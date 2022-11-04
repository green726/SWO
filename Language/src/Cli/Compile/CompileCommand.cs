using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

public class CompileCommand : Command<CompileCommandSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] CompileCommandSettings settings)
    {
        SWO.compileProject(settings);
        return 0;
    }
}
