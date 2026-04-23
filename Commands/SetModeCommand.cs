using Minecraft.Server.FourKit.Command;
using Minecraft.Server.FourKit.Entity;

namespace VeinMiner_LCE.Commands;

public class SetModeCommand : CustomCommand
{
    public string Label => "setmode";

    public string Usage => "/veinminer setmode <default|always|crouching|never>";

    public string Description => "Sets your personal VeinMiner mode.";

    public string[] Aliases => ["mode"];

    public bool onCommand(CommandSender sender, Command command, string label, string[] args)
    {
        if (sender is not Player player)
        {
            sender.sendMessage("Only players can use this command.");
            return true;
        }

        if (args.Length < 1)
        {
            sender.sendMessage($"Usage: {Usage}");
            return true;
        }

        if (!TryParseMode(args[0], out PlayerVeinMinerMode mode))
        {
            sender.sendMessage("Invalid mode. Use: default, always, crouching, never.");
            return true;
        }

        Veinminer.PlayerData.SetPlayerMode(player.getUniqueId(), mode);
        Veinminer.SavePlayerData();

        sender.sendMessage($"VeinMiner mode set to: {mode}");
        return true;
    }

    private static bool TryParseMode(string input, out PlayerVeinMinerMode mode)
    {
        mode = PlayerVeinMinerMode.Default;
        string value = input.Trim().ToLowerInvariant();

        switch (value)
        {
            case "default":
            case "d":
                mode = PlayerVeinMinerMode.Default;
                return true;
            case "always":
            case "a":
                mode = PlayerVeinMinerMode.Always;
                return true;
            case "crouching":
            case "crouch":
            case "c":
            case "sneak":
                mode = PlayerVeinMinerMode.Crouching;
                return true;
            case "never":
            case "n":
            case "off":
                mode = PlayerVeinMinerMode.Never;
                return true;
            default:
                return false;
        }
    }
}
