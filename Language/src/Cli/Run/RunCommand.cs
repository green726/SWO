using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

public class RunCommand : Command<RunCommandSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] RunCommandSettings settings)
    {
        CompileCommand compileCommand = new CompileCommand();
        compileCommand.Execute(context, settings);

        SWO.runProject(settings);

        return 0;
    }
}
