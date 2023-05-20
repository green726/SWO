using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;

public class TranspileCommand : Command<TranspileCommandSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] TranspileCommandSettings settings)
    {
        SWO.transpileProject(settings);
        return 0;
    }
}
