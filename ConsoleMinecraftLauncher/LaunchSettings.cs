namespace ConsoleMinecraftLauncher;

public class LaunchSettings
{
    public long? MaxMemory { get; set; }
    public long? MinMemory { get; set; }
    /// <summary>
    /// which server to join after launched.
    /// </summary>
    public string? ServerIp { get; set; }
}