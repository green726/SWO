using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using Tomlyn;

public class CompileCommand : Command<CompileCommandSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] CompileCommandSettings settings)
    {
        string[] files = System.IO.Directory.GetFiles(settings.path, "*.hproj");
        string tomlText = System.IO.File.ReadAllText(files[0]);
        ProjectInfo projectInfo = Toml.ToModel<ProjectInfo>(tomlText);

        HISS.compileProject(settings, projectInfo);
        return 0;
    }
}
