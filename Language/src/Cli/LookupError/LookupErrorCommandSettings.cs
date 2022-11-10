using Spectre.Console.Cli;
using System.ComponentModel;

public class LookupErrorCommandSettings : CommandSettings
{
    [Description("The error code you wish to lookup")]
    [CommandArgument(0, "[errorCode]")]
    public string errorCode { get; init; } = "";

    public LookupErrorCommandSettings()
    {
    }
}
