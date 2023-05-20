using Spectre.Console.Cli;
using System.ComponentModel;

public class TranspileCommandSettings : CommandSettings
{

    [Description("The path to the SWO project you wish to transpile - leave blank to use current directory")]
    [CommandOption("-p|--path")]
    public string path { get; init; } = "";

    [Description("Name of the SWO file you wish to transpile - leave blank to search for \"Main.swo\" in the current directory")]
    [CommandArgument(0, "[inputFile]")]
    public string file { get; init; } = "";

    [Description("The name of the resulting file")]
    [CommandArgument(1, "[resultFileName]")]
    [CommandOption("-o|--out")]
    public string resultFileName { get; set; } = "";

    [Description("SWO internal debug logging")]
    [CommandOption("-d|--debug")]
    public bool debugLogging { get; init; } = false;

    [Description("SWO internal debug logging")]
    [CommandArgument(0, "[targetConfigPath]")]
    public string targetConfigPath {get; init; } = "";

    // [Description("Package the necessary public components of the file's AST for distribution (used by SAP)")]
    // [CommandOption("--distribute-ast")]
    // public bool distributeAST { get; init; } = false;

    public TranspileCommandSettings()
    {
        if (path == "")
        {
            this.path = Environment.CurrentDirectory;
        }
    }
}
