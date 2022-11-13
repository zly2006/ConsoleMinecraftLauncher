using System.Diagnostics;

namespace ConsoleMinecraftLauncher;

public class Launcher
{
    private readonly FileInfo _jar;
    private readonly Account _account;
    private readonly DirectoryInfo _gameDir;
    private readonly LaunchSettings _settings;

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

    public Process? Launch()
    {
        CommandBuilder builder = new("java");
        if (!_account.Login())
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