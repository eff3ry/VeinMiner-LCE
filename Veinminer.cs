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

    public static Config CurrentConfig { get; private set; } = Config.CreateDefault();

    public override void onEnable()
    {
        ConfigPath = Path.Combine(GetPluginDirectory(), Config.DefaultFileName);
        ReloadConfig();

        FourKit.addListener(new BreakListener());
    }

    public override void onDisable()
    {
        SaveConfig();
    }

    public static void ReloadConfig()
    {
        CurrentConfig = Config.LoadOrCreate(ConfigPath);
    }

    public static void SaveConfig()
    {
        CurrentConfig.Save(ConfigPath);
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
