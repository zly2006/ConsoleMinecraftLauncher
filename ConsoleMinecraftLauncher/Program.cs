using ConsoleMinecraftLauncher;
using ConsoleMinecraftLauncher.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

List<Account> accounts = new();
ILogger logger = LoggerFactory.Create(config =>
{
    config.AddProvider(NullLoggerProvider.Instance);
}).CreateLogger("CML");

if (!Directory.Exists("accounts"))
{
    Directory.CreateDirectory("accounts");
}
foreach (var file in new DirectoryInfo("accounts").GetFiles("*.json"))
{
    try
    {
        Account account = Account.Load(file);
        accounts.Add(account);
        logger.LogInformation($"Loaded account {account.name} in type {account.type}");
    }
    catch { }
}

Java java = new Java("java");



Console.WriteLine("Hello, World!");
