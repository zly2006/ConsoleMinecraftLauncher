﻿using ConsoleMinecraftLauncher.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static System.Environment.SpecialFolder;

namespace ConsoleMinecraftLauncher;

static class Program
{
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

    public static List<Account> Accounts = new();

    public static readonly ILogger Logger = LoggerFactory
        .Create(config => { config.AddProvider(NullLoggerProvider.Instance); })
        .CreateLogger("CML");

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

        Java java = new Java("java");
        var dir = new DirectoryInfo(GetOfficialMinecraftPath());
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
                .ForEach(x => javaPath.Add(x.FullName));
        }

        var path = (Environment.GetEnvironmentVariable("PATH") ?? "")
            .Split(':')
            .Concat(javaPath)
            .Where(x => x != "")
            .Select(x => new DirectoryInfo(x))
            .Where(x => x.Exists)
            .Select(x => x.GetFiles()
                .Where(file => file.Name == "java")
                .Select(file => file.FullName)
            )
            .FlatMap();
        foreach (var j in path)
        {
            Console.WriteLine(j);
        }

        if (dir.Exists)
        {
        }
        else
        {
            dir.Create();
        }

        Console.WriteLine($"Default java version is {java.MajorVersion}.{java.MinorVersion}");
    }
}