using Minecraft.Server.FourKit.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace VeinMiner_LCE.Commands;

public interface CustomCommand : CommandExecutor
{
    public abstract string Label { get; }
    public abstract string Usage { get; }
    public abstract string Description { get; }
    public abstract string[] Aliases { get; }

    //bool onCommand(CommandSender sender, Command command, string label, string[] args);
}
