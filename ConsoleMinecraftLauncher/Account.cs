using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace ConsoleMinecraftLauncher;

public class Account
{
    public enum Type
    {
        Microsoft,
        Yggdrasil,
        Offline
    }
    public enum State
    {
        OK,
        Unverified,
        Expired,
    }

    public Type type;
    public string name;
    /// <summary>
    /// The Yggdrasil server
    /// </summary>
    internal string yggdrssilHost = "";
    /// <summary>
    /// for <see cref="Type.Microsoft"/> and <see cref="Type.Yggdrasil"/>
    /// </summary>
    internal string accessToken = "";

    internal string clientToken = "";
    internal string uuid = "";
    
    public State state = State.Unverified;

    /// <summary>
    /// create a account
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="login">if ture, the program will start the login process</param>
    public Account(Type type, string name, bool login = true)
    {
        this.type = type;
        this.name = name;
        if (type == Type.Microsoft)
        {
            
        }
        else if (type == Type.Yggdrasil)
        {
            
        }
        else
        {
            state = State.OK;
        }
    }

    public async Task<bool> Login()
    {
        if (type == Type.Offline)
        {
            return true;
        }
        if (type == Type.Yggdrasil)
        {
            var ret = new List<Account>();
            HttpResponseMessage response;
            Console.WriteLine($"Logging in {yggdrssilHost}...");
            var password = ReadPassword();
            using (var client = new HttpClient())
            {
                var jo = new JsonObject
                {
                    ["agent"] = new JsonObject
                    {
                        ["name"] = "Minecraft",
                        ["version"] = 1
                    },
                    ["username"] = name,
                    ["password"] = password,
                    ["requestUser"] = true
                };
                var content = JsonContent.Create(jo);
                response = await client.PostAsync(yggdrssilHost + "/authenticate", content);
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = (await response.Content.ReadFromJsonAsync<JsonObject>())!;
                ret.Add(new Account(Type.Yggdrasil, name, false)
                {
                    accessToken = json["accessToken"]!.GetValue<string>(),
                    clientToken = json["clientToken"]!.GetValue<string>(),
                });
            }
            else
            {
                throw new Exception("Authentication failed");
            }
        }

        return false;
    }

    /// <summary>
    /// Refresh this account (see https://wiki.vg/Legacy_Mojang_Authentication#Refresh)
    /// 
    /// if this account is <see cref="State.Unverified"/>, login.
    /// </summary>
    /// <returns>true if succeeded. otherwise, you should check your network or re-login.</returns>
    public async Task<bool> Refresh()
    {
        if (type == Type.Offline)
        {
            return true;
        } 
        if (type == Type.Yggdrasil)
        {
            using (var client = new HttpClient())
            {
                var jo = new JsonObject
                {
                    ["accessToken"] = accessToken,
                    ["clientToken"] = clientToken,
                    ["requestUser"] = true,
                    ["selectedProfile"] = new JsonObject
                    {
                        ["id"] = uuid,
                        ["name"] = name
                    }
                };
                var response = await client.PostAsync(yggdrssilHost + "/refresh", JsonContent.Create(jo));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var resJson = await response.Content.ReadFromJsonAsync<JsonObject>();
                    accessToken = resJson!["accessToken"]!.GetValue<string>();
                    clientToken = resJson["clientToken"]!.GetValue<string>();
                    var user = resJson["user"]!;
                    //todo
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// save this account.
    /// </summary>
    public void Save(DirectoryInfo directory)
    {
        
    }

    public static Account Load(FileInfo file)
    {
        return new Account(Type.Offline, "");
    }

    public static string ReadPassword()
    {
        int top = Console.CursorTop;
        int left = Console.CursorLeft;
        StringBuilder sb = new StringBuilder();
        var key = Console.ReadKey();
        while (key.Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                    Console.SetCursorPosition(left + sb.Length, top);
                    Console.Write("  ");
                }
                Console.SetCursorPosition(left + sb.Length, top);
            }
            else
            {
                sb.Append(key.KeyChar);
                Console.SetCursorPosition(left + sb.Length - 1, top);
                Console.Write("*");
            }
            key = Console.ReadKey();
        }
        Console.WriteLine();
        return sb.ToString();
    }
}