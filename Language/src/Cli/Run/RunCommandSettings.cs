using Spectre.Console.Cli;
using System.ComponentModel;

public class RunCommandSettings : CommandSettings
{
    public CompileCommandSettings compileCommandSettings { get; set; } = new CompileCommandSettings();

    [Description("Arguments to pass into the SWO project")]
    [CommandOption("-a|--args")]
    public string arguments { get; init; } = "";

}
