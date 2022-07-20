using Spectre.Console.Cli;
using System.ComponentModel;

public class CompileCommandSettings : CommandSettings
{

    [Description("The path to the HISS project you wish to compile - leave blank to use current directory")]
    [CommandOption("-p|--path")]
    public string path { get; init; } = "";

    [Description("Name of the entry HISS file you wish to compile - leave blank to search for \"Main.hiss\" in the current directory")]
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

    public FileType resultFileType = FileType.NativeExecutable;

    public CompileCommandSettings()
    {
        this.targetOSName = Util.checkOs();
        Enum.TryParse(resultFileTypeStr, out FileType resultFileType);
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
