using Minecraft.Server.FourKit.Command;
using System.Collections.Generic;
using System.Linq;

namespace VeinMiner_LCE.Commands;

public class RootCommand : CommandExecutor
{
    private readonly Dictionary<string, CustomCommand> _commands = new(StringComparer.OrdinalIgnoreCase);

    public RootCommand()
    {
        Register(new HelpCommand());
        Register(new SetModeCommand());
        Register(new ReloadCommand());
    }

    public bool onCommand(CommandSender sender, Command command, string label, string[] args)
    {
        string subcommandLabel = args.Length > 0 ? args[0].ToLower() : "help";
        string[] forwardedArgs = args.Length > 1 ? args.Skip(1).ToArray() : [];

        if (_commands.TryGetValue(subcommandLabel, out CustomCommand? subcommand))
        {
            return subcommand.onCommand(sender, command, label, forwardedArgs);
        }

        sender.sendMessage($"Unknown subcommand '{subcommandLabel}'. Use /{label} help.");
        return true;
    }

    private void Register(CustomCommand command)
    {
        _commands[command.Label] = command;
        foreach (string alias in command.Aliases)
        {
            _commands[alias] = command;
        }
    }

}
