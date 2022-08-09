using Spectre.Console.Cli;
using System.ComponentModel;


public class NewProjectSettings : CommandSettings
{
    [Description("The SWO template to use for this project")]
    [CommandArgument(0, "[template]")]
    public string templateStr { get; init; } = "Blank";

    public Template template;

    [Description("The name of the SWO project you wish to create")]
    [CommandArgument(1, "[name]")]
    public string projectName { get; init; } = "Blank";

    [Description("The path of the SWO project")]
    [CommandOption("-p|--path")]
    public string projectPath { get; init; }

    public NewProjectSettings()
    {
        this.template = new Template(templateStr);
        this.projectPath = Environment.CurrentDirectory;
    }

    public static explicit operator ProjectInfo(NewProjectSettings settings)
    {
        ProjectInfo projectInfo = new ProjectInfo();
        projectInfo.path = settings.projectPath;
        projectInfo.template = settings.template;
        projectInfo.projectName = settings.projectName;
        projectInfo.findEntryFileFromTemplate();

        return projectInfo;
    }

}
