using System.Diagnostics;
using System.Text;

namespace ConsoleMinecraftLauncher;

/// <summary>
/// build aa command to launch mc.
/// </summary>
public class CommandBuilder
{
    /// <summary>
    /// args
    /// </summary>
    private List<string> _args = new();
    /// <summary>
    /// java path
    /// </summary>
    private string _target;
    /// <summary>
    /// some attributes or vm options
    /// </summary>
    private Dictionary<string, string> kv = new();
    public CommandBuilder(string target)
    {
        _target = target;
    }

    public void SetVMOption(string name, object? value)
    {
        if (value == null)
        {
            return;
        }
        kv.Add($"-D{name}=", value.ToString()!);
    }

    public void SetKV(string key, string value)
    {
        kv.Add(key, value);
    }

    public void AddArg(string arg)
    {
        if (arg.StartsWith("\""))
        {
            _args.Add(arg);
        }
        else
        {
            _args.Add($"\"{arg}\"");
        }
    }

    /// <summary>
    /// build <see cref="_args"/> and <see cref="kv"/>, without java path.
    /// </summary>
    /// <returns></returns>
    public string BuildArgs()
    {
        StringBuilder builder = new(string.Join(' ', _args));
        foreach (var pair in kv)
        {
            builder.Append(pair.Key);
            builder.Append(pair.Value);
        }
        return builder.ToString();
    }

    public Process? Start()
    {
        return Process.Start(new ProcessStartInfo()
        {
            FileName = _target,
            Arguments = BuildArgs()
        });
    }
}