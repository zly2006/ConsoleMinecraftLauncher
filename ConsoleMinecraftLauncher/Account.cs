namespace ConsoleMinecraftLauncher;

public class Account
{
    public enum Type
    {
        Microsoft,
        Yggdrasil,
        Offline
    }

    public Type type;
    public string name;

    public Account(Type type, string name)
    {
        this.type = type;
        this.name = name;
    }

    public bool Login()
    {
        if (type == Type.Offline)
        {
            return true;
        }

        return false;
    }

    public void Save()
    {
        
    }

    public static Account Load(FileInfo file)
    {
        return new Account(Type.Offline, "");
    }
}