using Spectre.Console.Cli;
using System.ComponentModel;


public class NewProjectSettings : CommandSettings
{
    [Description("The HISS template to use for this project")]
    [CommandArgument(0, "[template]")]
    public string templateStr { get; init; } = "Blank";

    public Template template;

    [Description("The name of the HISS project you wish to create")]
    [CommandArgument(1, "[name]")]
    public string projectName { get; init; }

    [Description("The path of the HISS project")]
    [CommandOption("-p|--path")]
    public string projectPath { get; init; }

    public NewProjectSettings()
    {
        Console.WriteLine("start of new project settings constructor");
        this.template = new Template(templateStr);
        this.projectName = "my HISS project";
        this.projectPath = Environment.CurrentDirectory;
        Console.WriteLine("end of new project settings constructor");
    }

    public static explicit operator ProjectInfo(NewProjectSettings settings)
    {
        ProjectInfo projectInfo = new ProjectInfo();
        projectInfo.path = settings.projectPath;
        projectInfo.name = settings.projectName;
        projectInfo.template = settings.template;
        projectInfo.entryFile = settings.template.entryFile;

        return projectInfo;
    }

}
