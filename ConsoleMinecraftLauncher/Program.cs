using System.Reflection.Metadata;
using ConsoleMinecraftLauncher.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using static System.Environment.SpecialFolder;

namespace ConsoleMinecraftLauncher;

static class Program
{
    public static List<Account> Accounts = new();

    public static readonly ILogger Logger = LoggerFactory
        .Create(config =>
        {
            var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>("", null);
            var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new []{ configureNamedOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
            var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());
            config.AddProvider(new ConsoleLoggerProvider(optionsMonitor));
        })
        .CreateLogger("CML");

    public static List<Java> Javas = new();

    public static List<MinecraftClient> Clients = new();

    static DirectoryInfo? SubDirectoryOrNull(this DirectoryInfo directory, params string[] name)
    {
        if (!directory.Exists)
        {
            return null;
        }

        if (name.Length == 1 && name[0] == ".")
        {
            return directory;
        }

        foreach (var s in name)
        {
            directory = new DirectoryInfo(Path.Combine(directory.FullName, s));
        }

        if (!directory.Exists)
        {
            return null;
        }

        return directory;
    }

    static IEnumerable<T> FlatMap<T>(this IEnumerable<IEnumerable<T>> enumerable)
    {
        List<T> ret = new();
        foreach (var e in enumerable)
        {
            ret.AddRange(e);
        }

        return ret;
    }

    public static string GetOfficialMinecraftPath()
    {
        if (OperatingSystem.IsLinux())
        {
            return Path.Combine(Environment.GetFolderPath(UserProfile), ".minecraft");
        }

        if (OperatingSystem.IsMacOS())
        {
            return Path.Combine(Environment.GetFolderPath(UserProfile),
                "Library",
                "Application Support",
                "minecraft");
        }

        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(Environment.GetFolderPath(ApplicationData), ".minecraft");
        }

        return "";
    }

    public static void Main(string[] args)
    {
        
        if (!Directory.Exists("accounts"))
        {
            Directory.CreateDirectory("accounts");
        }

        foreach (var file in new DirectoryInfo("accounts").GetFiles("*.json"))
        {
            try
            {
                Account account = Account.Load(file);
                Accounts.Add(account);
                Logger.LogInformation($"Loaded account {account.name} in type {account.type}");
            }
            catch
            {
            }
        }
        
        var minecraftDir = new DirectoryInfo(GetOfficialMinecraftPath());
        List<string> javaPath = new();
        if (OperatingSystem.IsMacOS())
        {
            javaPath.Add("/Library/Internet Plug-Ins/JavaAppletPlugin.plugin/Contents/Home/bin");
            javaPath.Add(
                "/Applications/Xcode.app/Contents/Applications/Application Loader.app/Contents/MacOS/itms/java/bin");
            new DirectoryInfo[]
                {
                    new("/Library/Java/JavaVirtualMachines"),
                    new(Environment.GetFolderPath(UserProfile) + "/Library/Java/JavaVirtualMachines"),
                    new("/System/Library/Java/JavaVirtualMachines")
                }
                .Where(x => x.Exists)
                .Select(path =>
                    path.GetDirectories()
                        .Select(x => new[]
                            {
                                x.SubDirectoryOrNull("Contents", "Home", "bin"),
                                x.SubDirectoryOrNull("Contents", "Home", "jre", "bin")
                            }
                            .Where(x => x is not null && x.Exists)
                        )
                        .FlatMap()
                        .Where(x => x is not null && x.Exists))
                .FlatMap()
                .Where(x => x is not null && x.Exists)
                .ToList()
                .ForEach(x => javaPath.Add(x!.FullName));
        }
        else if (OperatingSystem.IsWindows())
        {
            List<string> drivePath = new();
            DriveInfo[] driveInfos = DriveInfo.GetDrives();
            foreach (var item in driveInfos)
            {
                drivePath.Add(item.Name+"Program Files\\Java");
                drivePath.Add(item.Name+"Program Files\\java");
                drivePath.Add(item.Name+"Program Files(x86)\\Java");
                drivePath.Add(item.Name+"Program Files(x86)\\java");
                drivePath.Add(item.Name+"\\Java");
                drivePath.Add(item.Name+"\\java");
            }
            drivePath.Select(x => new DirectoryInfo(x))
                     .Where(x => x.Exists)
                     .Select(path 
                         => path.GetDirectories().Select(x => new[]
                             {
                                 x.SubDirectoryOrNull("Contents", "Home", "bin"),
                                 x.SubDirectoryOrNull("Contents", "Home", "jre", "bin")
                             }
                             .Where(x => x is not null && x.Exists)
                     ).FlatMap()
                             .Where(x => x is not null && x.Exists))
                     .FlatMap()
                     .Where(x => x is not null && x.Exists)
                     .ToList()
                     .ForEach(x => javaPath.Add(x!.FullName));
        }
        else if (OperatingSystem.IsLinux())
        {
            new DirectoryInfo[]
                {
                    new("/usr/java"),
                    new("/usr/lib/jvm"),
                    new("/usr/lib32/jvm")
                }
                .Where(x => x.Exists)
                .Select(x =>
                    x.GetDirectories()
                        .Select(dir => Path.Combine(dir.FullName, "bin", "java"))
                        .Where(File.Exists))
                .FlatMap()
                .ToList()
                .ForEach(x => javaPath.Add(x));
        }
        else
        {
            Logger.LogError("Unsupported OS");
            return;
        }

        Console.WriteLine("Loading java...");
        (Environment.GetEnvironmentVariable("PATH") ?? "")
            // in windows, path separator is ';'
            .Split(OperatingSystem.IsWindows() ? ';' : ':')
            .Concat(javaPath)
            .Where(x => x != "")
            .Select(x => new DirectoryInfo(x))
            .Where(x => x.Exists)
            .Select(x => x.GetFiles()
                .Where(file => file.Name == "java")
                .Select(file => file.FullName)
            )
            .FlatMap()
            .Select<string, Action>(p =>
            {
                return () =>
                {
                    try
                    {
                        var java = new Java(p);
                        Javas.Add(java);
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning(e, "Failed to load java");
                        throw;
                    }
                };
            })
            .Select(Task.Run)
            .ToList()
            .ForEach(task => task.Wait());

        Console.WriteLine($"Loaded {Javas.Count} java executables!");
        foreach (var java in Javas)
            Console.WriteLine($"{java.Path} {java.MajorVersion}.{java.MinorVersion}");

        if (minecraftDir.Exists)
        {
            var configDir = minecraftDir.SubDirectoryOrNull(".cml") ?? minecraftDir.CreateSubdirectory(".cml");
            var accountsDir = configDir.SubDirectoryOrNull("accounts") ?? configDir.CreateSubdirectory("accounts");
            var versionDir = minecraftDir.SubDirectoryOrNull("versions") ?? minecraftDir.CreateSubdirectory("versions");
            foreach (var directory in versionDir.GetDirectories())
            {
                if (File.Exists(Path.Combine(directory.FullName, directory.Name + ".json")))
                {
                    var jsonFile = new FileInfo(Path.Combine(directory.FullName, directory.Name + ".json"));
                    Clients.Add(new MinecraftClient(jsonFile, new LaunchSettings(), minecraftDir));
                }
            }
        }
        else
        {
            minecraftDir.Create();
        }

        MicrosoftLogin login = new();
        var password = Account.ReadPassword();
        while (true) { }
    }
}