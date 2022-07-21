using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

public class NewProjectCommand : Command<NewProjectSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] NewProjectSettings settings)
    {
        Console.WriteLine("desired path:" + settings.projectPath);

        //NOTE: below is to copy config file - we dont do this b/c prjects will (by default) use the global config
        // string configPath = Util.installPath + "/Resources/Config/config.toml";
        // File.Copy(configPath, $@"{settings.projectPath}/config.toml");
        ProjectInfo projectInfo = (ProjectInfo)settings;
        // projectInfo.checkPath();
        projectInfo.setConfig();
        projectInfo.write();

        return 0;
    }
}
