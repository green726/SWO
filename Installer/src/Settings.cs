using Spectre.Console.Cli;
using System.ComponentModel;

public class Settings : CommandSettings
{
    [Description(@"Path to install HISS. Defaults to %UserProfile%\.HISS on windows and ~/.HISS on Linux")]
    [CommandArgument(0, "[installPath]")]
    public string? installPath { get; init; }

    [Description("Uninstall HISS")]
    [CommandOption("--u|--uninstall")]
    [DefaultValue(false)]
    public bool uninstall { get; init; }

    [Description("Install HIP (Highly Inefficient Packages) - The HISS Package Manager")]
    [CommandOption("--hip|--installhip")]
    [DefaultValue(true)]
    public bool installHIP { get; init; }

    [Description("Install the HISS Resources - this is required for many HISS features however you can go without installing it for a greater degree of customization.")]
    [CommandOption("--resources|--r")]
    [DefaultValue(true)]
    public bool installResources { get; init; }

    public Settings()
    {
        this.installPath = Util.evaluatePath();
    }
}

