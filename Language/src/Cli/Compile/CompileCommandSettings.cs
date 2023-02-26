using Spectre.Console.Cli;
using System.ComponentModel;

public class CompileCommandSettings : CommandSettings
{

    [Description("The path to the SWO project you wish to compile - leave blank to use current directory")]
    [CommandOption("-p|--path")]
    public string path { get; init; } = "";

    [Description("Name of the SWO file you wish to compile - leave blank to search for \"Main.swo\" in the current directory")]
    [CommandArgument(0, "[inputFile]")]
    public string file { get; init; } = "";

    [Description("The name of the target OS for the resulting compiled file - leave blank to use current OS")]
    [CommandOption("-t|--target")]
    public string targetOSName { get; init; } = "";

    [Description("The type of the resulting file")]
    [CommandOption("-r|--result")]
    public string resultFileTypeStr { get; init; } = "NativeExecutable";

    [Description("The name of the resulting file")]
    [CommandArgument(1, "[resultFileName]")]
    [CommandOption("-o|--out")]
    public string resultFileName { get; set; } = "";

    [Description("SWO internal debug logging")]
    [CommandOption("-d|--debug")]
    public bool debugLogging { get; init; } = false;

    [Description("Opimization level you wish to use for compilation. Values can be from 0 (least) to 2 (most). The optimization levels coincide (roughly) with Clang's optimization levels.")]
    [CommandOption("-O|--optimize")]
    public int optimizationLevel { get; init; } = 1;

    // [Description("Package the necessary public components of the file's AST for distribution (used by SAP)")]
    // [CommandOption("--distribute-ast")]
    // public bool distributeAST { get; init; } = false;

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
    AST,
}
