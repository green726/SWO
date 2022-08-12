using Spectre.Console.Cli;
using System.ComponentModel;

public class CompileCommandSettings : CommandSettings
{

    [Description("The path to the SWO project you wish to compile - leave blank to use current directory")]
    [CommandOption("-p|--path")]
    public string path { get; init; } = "";

    [Description("Name of the entry SWO file you wish to compile - leave blank to search for \"Main.swo\" in the current directory")]
    [CommandOption("-f|--file")]
    public string file { get; init; } = "";

    [Description("The name of the target OS for the resulting compiled file - leave blank to use current OS")]
    [CommandOption("-t|--target")]
    public string targetOSName { get; init; } = "";

    [Description("The type of the resulting file")]
    [CommandOption("-r|--result")]
    public string resultFileTypeStr { get; init; } = "NativeExecutable";

    [Description("The name of the resulting file")]
    [CommandArgument(0, "[resultFileName]")]
    public string resultFileName { get; set; } = "";

    [Description("SWO internal debug logging")]
    [CommandOption("-d|--debug")]
    public bool debugLogging { get; init; } = false;

    public FileType resultFileType
    {
        get
        {
            return (FileType)Enum.Parse(typeof(FileType), resultFileTypeStr, true);
        }
        set
        {
        }
    }

    public CompileCommandSettings()
    {
        this.targetOSName = Util.checkOs();

        if (path == "")
        {
            this.path = Environment.CurrentDirectory;
        }
    }
}

public enum FileType
{
    Assembly,
    Object,
    Binary,
    LLVMIR,
    NativeExecutable,
}
