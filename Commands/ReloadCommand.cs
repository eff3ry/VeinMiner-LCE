using Minecraft.Server.FourKit;
using Minecraft.Server.FourKit.Command;
using Minecraft.Server.FourKit.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace VeinMiner_LCE.Commands;

internal class ReloadCommand : CustomCommand
{
    public string Label => "reload";
    public string Usage => "/veinminer reload";
    public string Description => "Reloads VeinMiner configuration and data.";
    public string[] Aliases => [];
    public bool onCommand(CommandSender sender, Command command, string label, string[] args)
    {
        if (sender is not ConsoleCommandSender) // only callable from console
        {
            return false;
        }

        Veinminer.ReloadConfig();
        Veinminer.ReloadPlayerData();
        sender.sendMessage(ChatColor.GREEN + "VeinMiner configuration and player data reloaded successfully.");

        return true;
    }
}
