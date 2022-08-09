using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using Tomlyn;

public class CompileCommand : Command<CompileCommandSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] CompileCommandSettings settings)
    {
        Swo.compileProject(settings);
        return 0;
    }
}
