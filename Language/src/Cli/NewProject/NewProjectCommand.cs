using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

public class NewProjectCommand : Command<NewProjectSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] NewProjectSettings settings)
    {
        string configPath = OperatingSystem.IsWindows() ? Environment.ExpandEnvironmentVariables(@$"%USERPROFILE%\.HISS\Resources\Config\config.toml") : Environment.ExpandEnvironmentVariables($"%HOME%/.HISS/Resources/Config/config.toml");
        Console.WriteLine("desired path:" + settings.projectPath);
        Util.copyDirectory(settings.template.path, settings.projectPath, true, true);
        File.Copy(configPath, $@"{settings.projectPath}/config.toml");
        ProjectInfo projectInfo = (ProjectInfo)settings;
        projectInfo.addFilesFromDir(settings.template.path);
        projectInfo.write();

        return 0;
    }
}
