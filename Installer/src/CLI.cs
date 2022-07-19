using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

public class CLI
{
    CommandApp<InstallCommand> app;
    public CLI()
    {
        app = new CommandApp<InstallCommand>();
    }

}

public class InstallCommand : Command<Settings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        Util.figureOutSettings(settings);

        return 0;
    }
}

