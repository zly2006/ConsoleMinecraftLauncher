using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ConsoleMinecraftLauncher.Utils;

public class Java
{
    public string Path { get; set; }
    public int MajorVersion { get; set; }
    public string VersionString { get; set; }
    public int Arch { get; set; }

    public bool FitMinecraftVersion(Version version)
    {
        if (version.Major != 1)
        {
            throw new Exception("Not Minecraft!");
        }
        if (version.Type == Version.VersionType.Other)
        {
            return true;
        }
        if (version.Type != Version.VersionType.Full && version.Type != Version.VersionType.Snapshot)
        {
            return false;
        }
        return version.Minor switch
        {
            < 17   => MajorVersion is >= 8 and < 17,
            17     => MajorVersion == 16,
            >= 18  => MajorVersion >= 17,
        };
    }

    public Java(string path)
    {
        Path = path;
        Process verProcess = Process.Start(new ProcessStartInfo()
        {
            FileName = path,
            Arguments = "-XshowSettings -version",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        }) ?? throw new InvalidOperationException("Failed to get java version!");
        verProcess.Start();
        verProcess.WaitForExit();
        var output = verProcess.StandardOutput.ReadToEnd() + verProcess.StandardError.ReadToEnd();
        VersionString = Regex.Match(output, "java\\.version\\s*=\\s*\"?(?<version>[0-9]+\\.[0-9]+\\.[0-9]+_?[0-9]*)").Groups["version"].Value;
        var arch = Regex.Match(output, "sun\\.arch\\.data\\.model\\s*=\\s*([0-9]+)").Groups[1].Value;
        Arch = Convert.ToInt32(arch);
        MajorVersion = Convert.ToInt32(VersionString.Split('.')[0]);
        var minor = Convert.ToInt32(VersionString.Split('.')[1]);
        if (MajorVersion == 1)
        {
            MajorVersion = minor;
        }
    }
}