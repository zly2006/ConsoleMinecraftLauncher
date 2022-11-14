using System.Diagnostics;
using ConsoleMinecraftLauncher.Utils;
using Microsoft.Extensions.Logging;
using Version = System.Version;

namespace ConsoleMinecraftLauncher;

/// <summary>
/// 
/// </summary>
public class Launcher
{
    /// <summary>
    /// The minecraft client
    /// </summary>
    private readonly FileInfo _jar;
    /// <summary>
    /// Selected account
    /// </summary>
    private readonly Account _account;
    /// <summary>
    /// Launch mc in which directory (for version isolation)
    /// </summary>
    private readonly DirectoryInfo _gameDir;
    /// <summary>
    /// Setting for this version
    /// </summary>
    private readonly LaunchSettings _settings;
    /// <summary>
    /// The version of this minecraft. The <see cref="Utils.Version"/> class includes mc version, mod loader type, mc type, etc.
    /// </summary>
    private Utils.Version _minecraftVersion;
    /// <summary>
    /// true if you want to ignore warnings.
    /// </summary>
    public bool ForceLaunch { get; set; } = false;

    public Launcher(FileInfo jar, Account account, DirectoryInfo gameDir, LaunchSettings settings)
    {
        if (!jar.Exists)
        {
            throw new ArgumentException("Jar file does not exist.");
        }

        _jar = jar;
        _account = account;
        _gameDir = gameDir;
        _settings = settings;

        if (!gameDir.Exists)
        {
            gameDir.Create();
        }
    }

    public Process? Launch(Java java)
    {
        if (!ForceLaunch && java.FitMinecraftVersion(_minecraftVersion))
        {
            Console.Write("Warning: your java version is too new/out of date for the mc version, launch anyway? (y/N)");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                return null;
            }
        }
        CommandBuilder builder = new(java.Path);
        if (!_account.Refresh())
        {
            return null;
        }

        if (_settings.MaxMemory is not null)
        {
            builder.SetKV("-Xmx", _settings.MaxMemory.ToString()!);
        }

        builder.SetVMOption("minecraft.client.jar",_jar.FullName);
        builder.SetVMOption("dock:name", _settings);
        
        
        // Fix RCE of log4j2
        builder.SetVMOption("java.rmi.server.useCodebaseOnly", "true");
        builder.SetVMOption("com.sun.jndi.rmi.object.trustURLCodebase", "false");
        builder.SetVMOption("com.sun.jndi.cosnaming.object.trustURLCodebase", "false");

        if (_settings.ServerIp is not null)
        {
            string ip = _settings.ServerIp.Split(':')[0];
            int port = _settings.ServerIp.Contains(':')
                ? Convert.ToInt16(_settings.ServerIp.Split(':')[1])
                : 25565;
            builder.AddArg("--server");
            builder.AddArg(ip);
            builder.AddArg("--port");
            builder.AddArg(port.ToString());
        }
        return builder.Start();
    }
}