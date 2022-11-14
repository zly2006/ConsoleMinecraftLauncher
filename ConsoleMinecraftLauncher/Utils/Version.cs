namespace ConsoleMinecraftLauncher.Utils;

/// <summary>
/// Represents a minecraft version.
///
/// 
/// </summary>
public class Version
{
    public enum VersionType
    {
        Classic,
        InDev,
        InfDev,
        Alpha,  
        Beta,
        Full,
        Snapshot,
        AprilFoolJoke,
        Other
    }
    public enum ModLoaderType
    {
        Vanilla,
        Forge,
        Fabric,
        LiteLoader
    }
    public ModLoaderType ModLoader { get; set; }
    public VersionType Type { get; set; }
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }
    public string? VersionString { set; private get; } = null;

    public string Identifier
    {
        get
        {
            if (VersionString == null)
            {
                return $"{Major}.{Minor}.{Patch}";
            }
            else return VersionString;
        }
    }
}