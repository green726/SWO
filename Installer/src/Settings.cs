using Spectre.Console.Cli;
using System.ComponentModel;

public class Settings : CommandSettings
{
    [Description(@"Path to install SWO. Defaults to %UserProfile%\.SWO on windows and ~/.SWO on Linux")]
    [CommandArgument(0, "[installPath]")]
    public string? installPath { get; init; }

    [Description("Uninstall SWO")]
    [CommandOption("-u|--uninstall")]
    [DefaultValue(false)]
    public bool uninstall { get; init; }

    [Description("Dont Install SAP (Highly Inefficient Packages) - The SWO Package Manager")]
    [CommandOption("--nohip|--installhip")]
    [DefaultValue(false)]
    public bool dontInstallSAP { get; init; }

    [Description("Dont the SWO Resources - this is required for many SWO features however you can go without installing it for a greater degree of customization.")]
    [CommandOption("--nor|--noresources")]
    [DefaultValue(false)]
    public bool dontInstallResources { get; init; }

    [Description("The SWO version - you can use a specific version number or specify a branch from the github - set to \"stable\" for the latest stable release")]
    [CommandOption("-v|--version")]
    public string version { get; init; } = "stable";

    public Settings()
    {
        this.installPath = Util.evaluatePath();
        // Console.WriteLine("path: " + this.installPath);
    }
}

