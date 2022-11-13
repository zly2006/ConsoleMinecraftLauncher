namespace ConsoleMinecraftLauncher.Utils;

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
}