using Spectre.Console.Cli;
using System.ComponentModel;

public class Settings : CommandSettings
{
    [Description(@"Path to install HISS. Defaults to %UserProfile%\.HISS on windows and ~/.HISS on Linux")]
    [CommandArgument(0, "[installPath]")]
    public string? installPath { get; init; }

    [Description("Uninstall HISS")]
    [CommandOption("-u|--uninstall")]
    [DefaultValue(false)]
    public bool uninstall { get; init; }

    [Description("Dont Install HIP (Highly Inefficient Packages) - The HISS Package Manager")]
    [CommandOption("--nohip|--installhip")]
    [DefaultValue(false)]
    public bool dontInstallHIP { get; init; }

    [Description("Dont the HISS Resources - this is required for many HISS features however you can go without installing it for a greater degree of customization.")]
    [CommandOption("--nor|--noresources")]
    [DefaultValue(false)]
    public bool dontInstallResources { get; init; }

    public Settings()
    {
        this.installPath = Util.evaluatePath();
    }
}

