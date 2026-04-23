using Minecraft.Server.FourKit.Command;

namespace VeinMiner_LCE.Commands;

public class HelpCommand : CustomCommand
{
    public string Label => "help";

    public string Usage => "/veinminer help";

    public string Description => "Shows available VeinMiner commands.";

    public string[] Aliases => ["?"];

    public bool onCommand(CommandSender sender, Command command, string label, string[] args)
    {
        sender.sendMessage("VeinMiner LCE - Commands:");
        sender.sendMessage("/veinminer setmode <default | always | crouching | never>");
        sender.sendMessage("/veinminer help");
        return true;
    }
}
