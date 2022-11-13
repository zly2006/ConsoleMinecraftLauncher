using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ConsoleMinecraftLauncher.Utils;

public class Java
{
    public string Path { get; set; }
    public int MajorVersion { get; set; }
    public int MinorVersion { get; set; }

    public bool FitMinecraftVersion(Version version)
    {
        if (version.Major != 1)
        {
            throw new Exception("Not Minecraft!");
        }

        return version.Minor switch
        {
            < 17   => MinorVersion is >= 8 and < 17,
            17     => MinorVersion == 16,
            >= 18  => MinorVersion >= 17,
        };
    }

    public Java(string path)
    {
        Path = path;
        Process verProcess = Process.Start(new ProcessStartInfo()
        {
            FileName = path,
            Arguments = "--version",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true
        }) ?? throw new InvalidOperationException("Failed to get java version!");
        verProcess.Start();
        verProcess.WaitForExit();
        string versionString = verProcess.StandardOutput.ReadToEnd();
        versionString = Regex.Match(versionString, "\\d\\.\\d").Value;
        MajorVersion = Convert.ToInt32(versionString[..versionString.IndexOf('.')]);
        MinorVersion = Convert.ToInt32(versionString[(versionString.IndexOf('.') + 1)..]);
        if (MajorVersion != 1)
        {
            MinorVersion = MajorVersion;
            MajorVersion = 1;
            // 1.7 1.8 1.18, etc
        }
    }
}