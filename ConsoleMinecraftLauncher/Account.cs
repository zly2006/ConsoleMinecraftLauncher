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

    /// <summary>
    /// Refresh this account (see https://wiki.vg/Legacy_Mojang_Authentication#Refresh)
    /// 
    /// if this account is <see cref="State.Unverified"/>, login.
    /// </summary>
    /// <returns>true if succeeded. otherwise, you should check your network or re-login.</returns>
    public bool Refresh()
    {
        if (type == Type.Offline)
        {
            return true;
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
}