using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

public class NewProjectCommand : Command<NewProjectSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] NewProjectSettings settings)
    {

        Util.copyDirectory(settings.template.path, settings.projectPath, true, true);
        ProjectInfo projectInfo = (ProjectInfo)settings;
        projectInfo.addFilesFromDir(settings.template.path);
        projectInfo.write();

        return 0;
    }
}
