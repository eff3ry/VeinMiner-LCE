using Minecraft.Server.FourKit;
using Minecraft.Server.FourKit.Plugin;
using System.IO;

namespace VeinMiner_LCE;

public class Veinminer : ServerPlugin
{
    public override string name => "VeinMiner-LCE";
    public override string version => "1.0.0";
    public override string author => "effery";

    public static string ConfigPath { get; private set; } = Path.Combine(GetPluginDirectory(), Config.DefaultFileName);

    public static string PlayerDataPath { get; private set; } = Path.Combine(GetPluginDirectory(), PlayerDataStore.DefaultFileName);

    public static Config CurrentConfig { get; private set; } = Config.CreateDefault();

    public static PlayerDataStore PlayerData { get; private set; } = new();

    public override void onEnable()
    {
        ConfigPath = Path.Combine(GetPluginDirectory(), Config.DefaultFileName);
        PlayerDataPath = Path.Combine(GetPluginDirectory(), PlayerDataStore.DefaultFileName);
        ReloadConfig();
        ReloadPlayerData();

        FourKit.addListener(new BreakListener());

        FourKit.getCommand("veinminer").setExecutor(new Commands.RootCommand());
    }

    public override void onDisable()
    {
        SaveConfig();
        SavePlayerData();
    }

    public static void ReloadConfig()
    {
        CurrentConfig = Config.LoadOrCreate(ConfigPath);
    }

    public static void SaveConfig()
    {
        CurrentConfig.Save(ConfigPath);
    }

    public static void ReloadPlayerData()
    {
        PlayerData = PlayerDataStore.LoadOrCreate(PlayerDataPath);
    }

    public static void SavePlayerData()
    {
        PlayerData.Save(PlayerDataPath);
    }

    private static string GetPluginDirectory()
    {
        string? assemblyDirectory = Path.GetDirectoryName(typeof(Veinminer).Assembly.Location);
        if (!string.IsNullOrWhiteSpace(assemblyDirectory))
        {
            return assemblyDirectory;
        }

        return Path.Combine(AppContext.BaseDirectory, "plugins", "VeinMiner-LCE");
    }
}
