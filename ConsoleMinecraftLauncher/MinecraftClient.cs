using System.Diagnostics;
using System.Text.Json.Nodes;
using ConsoleMinecraftLauncher.Utils;
using Microsoft.Extensions.Logging;
using Version = System.Version;

namespace ConsoleMinecraftLauncher;

/// <summary>
/// represents a minecraft client
/// </summary>
public class MinecraftClient
{
    public bool Exists => File.Exists(_jar);

    /// <summary>
    /// The minecraft client path
    /// </summary>
    private readonly string _jar;

    /// <summary>
    /// Launch mc in which directory (for version isolation)
    /// </summary>
    private readonly DirectoryInfo _gameDir;

    /// <summary>
    /// .minecraft directory
    /// </summary>
    private readonly DirectoryInfo _minecraftDir;

    /// <summary>
    /// Setting for this version
    /// </summary>
    private readonly LaunchSettings _settings;

    /// <summary>
    /// The version of this minecraft. The <see cref="Utils.Version"/> class includes mc version, mod loader type, mc type, etc.
    /// </summary>
    private Utils.Version _minecraftVersion;

    private JsonObject _versionJson;

    /// <summary>
    /// true if you want to ignore warnings.
    /// </summary>
    public bool ForceLaunch { get; set; } = false;

    string GetJarName()
    {
        return (_versionJson["jar"] ?? _versionJson["id"]!).GetValue<string>() + ".jar";
    }

    public MinecraftClient(FileInfo jsonFile, LaunchSettings settings, DirectoryInfo minecraftDir)
    {
        if (!minecraftDir.Exists || !jsonFile.Exists)
        {
            throw new ArgumentException("Jar file does not exist.");
        }
        _versionJson = (JsonObject) JsonNode.Parse(File.ReadAllText(jsonFile.FullName))!;
        _jar = Path.Combine(jsonFile.Directory!.FullName, GetJarName());
        _gameDir = jsonFile.Directory!;
        _settings = settings;
        _minecraftDir = minecraftDir;
        //_minecraftVersion = 
    }

    public async Task<Process?> Launch(Java java, Account account)
    {
        var id = _versionJson["id"]!.GetValue<string>();
        if (!ForceLaunch && java.FitMinecraftVersion(_minecraftVersion))
        {
            Console.Write("Warning: your java version is too new/out of date for the mc version, launch anyway? (y/N)");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                return null;
            }
        }

        CommandBuilder builder = new(java.Path);
        if (!await account.Refresh())
        {
            return null;
        }

        builder.AddArg("-jar");
        builder.AddArg(_jar);
        BuildCommand(builder, new HashSet<string>()
        {
        }, new Dictionary<string, string>()
        {
            {"auth_player_name", account.name},
            {"version_name", id},
            {"game_directory", _gameDir.FullName},
            {"assets_root", ""},//todo
            {"assets_index_name",""},//todo
            {"auth_uuid", account.uuid},
            {"auth_access_token", account.accessToken},
            {"user_type",""},//todo: check document
            {"resolution_width", _settings.WindowWidth.ToString()},
            {"resolution_height", _settings.WindowHeight.ToString()}
        });

        if (_settings.MaxMemory is not null)
        {
            builder.SetKV("-Xmx", _settings.MaxMemory.ToString()!);
        }

        builder.SetVMOption("minecraft.client.jar", _jar);
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

        return builder.Start(true);
    }

    /// <summary>
    /// build arguments specified in version json
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="enabledFeatures"></param>
    /// <param name="replaceMap"></param>
    /// <exception cref="Exception"></exception>
    public void BuildCommand(CommandBuilder builder,
        ISet<string> enabledFeatures,
        IDictionary<string, string> replaceMap)
    {
        var id = _versionJson["id"]!.GetValue<string>();

        // add argument specified in json
        _versionJson["argument"]!.AsArray().ToList().ForEach(x =>
        {
            if (x is JsonValue)
            {
                var str = x.ToString();
                if (str.Contains("${"))
                {
                    var index = str.IndexOf("${", StringComparison.Ordinal);
                    var to = str.IndexOf("}", index, StringComparison.Ordinal);
                    var key = str.Substring(index + 2, to - index - 2);
                    if (replaceMap.ContainsKey(key))
                    {
                        str = str.Replace("${" + key + "}", replaceMap[key]);
                    }
                    else
                    {
                        throw new Exception("Key not found: " + key);
                    }
                }
            }
            else if (x is JsonObject)
            {
                var rules = x["rules"]!.AsArray();
                bool allow = false;
                foreach (var rule in rules)
                {
                    if (rule!["action"]!.ToString() == "allow")
                    {
                        if (enabledFeatures.Contains(rule["features"]!.ToString()))
                        {
                            allow = true;
                            break;
                        }
                    }
                    /* TODO: the deny key is not found in the json file,
                             we just guess that the deny key is the opposite of the allow key */
                    else if (rule!["action"]!.ToString() == "deny")
                    {
                        if (enabledFeatures.Contains(rule["features"]!.ToString()))
                        {
                            allow = false;
                            break;
                        }
                    }
                }

                if (allow)
                {
                    builder.AddArg(x["value"]!.ToString());
                }
            }
        });
    }
}