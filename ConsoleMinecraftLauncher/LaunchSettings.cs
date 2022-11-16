namespace ConsoleMinecraftLauncher;

public class LaunchSettings
{
    public long? MaxMemory { get; set; } = null;
    public long? MinMemory { get; set; } = null;
    public int? WindowWidth { get; set; } = null;
    public int? WindowHeight { get; set; } = null;

    /// <summary>
    /// which server to join after launched.
    /// </summary>
    public string? ServerIp { get; set; } = null;

    public bool Demp { get; set; } = false;
}