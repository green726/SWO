using System.Diagnostics;
using Tomlyn;
using Spectre.Console;

public static class SWO
{
    private static string fileContents;
    public static bool windows = false;
    public static ProjectInfo projectInfo;

    static void Main(string[] args)
    {
        Util.getSWOInstallPath();

        CLI cli = new CLI(args);
    }

    public static void compileProject(CompileCommandSettings settings)
    {
        DebugConsole.log = settings.debugLogging;

        // DebugConsole.Write(settings.resultFileType);
        string[] files = System.IO.Directory.GetFiles(settings.path, "*.sproj");

        if (files.Length == 0)
        {
            throw new ArgumentException("No SWO project files found in current directory");
        }

        string tomlText = System.IO.File.ReadAllText(files[0]);
        projectInfo = Toml.ToModel<ProjectInfo>(tomlText);
        projectInfo.setConfig();

        if (!settings.debugLogging)
        {
            AnsiConsole.Progress()
                .Columns(new ProgressColumn[] {
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new RemainingTimeColumn(),
            new SpinnerColumn(),
                }).Start(ctx =>
                {
                    var configTask = ctx.AddTask("Initializing config");
                    Config.initialize(projectInfo.configFilePath);
                    configTask.StopTask();
                    //TODO: check if its default or leave it alone
                    settings.resultFileName = projectInfo.projectName;

                    if (Config.settings.general.typo.enabled)
                    {
                        var typoTask = ctx.AddTask("Initializing typo checker");
                        Typo.initialize(typoTask);
                        typoTask.StopTask();
                    }

                    fileContents = System.IO.File.ReadAllText(projectInfo.entryFile.path);
                    var lexTask = ctx.AddTask("Lexing (tokenizing) the SWO code");
                    List<Util.Token> lexedContent = Lexer.lex(fileContents, lexTask);


                    var parseTask = ctx.AddTask("Parsing the SWO code");

                    List<Parser> nodes = Parser.startParsing(lexedContent, parseTask);

                    var moduleTask = ctx.AddTask("Initializing LLVM");
                    ModuleGen.GenerateModule(moduleTask);

                    var llvmTask = ctx.AddTask("Compiling to LLVM IR");
                    IRGen.generateIR(nodes, llvmTask);

                    var passTask = ctx.AddTask("Optimizing the LLVM IR");
                    IRGen.optimizeIR(passTask);

                    var exeCompileTask = ctx.AddTask("Compiling the LLVM IR to your desired output");
                    EXE.compileEXE(settings, exeCompileTask, true);

                });
        }
        else
        {
            Config.initialize(projectInfo.configFilePath);
            //TODO: check if its default or leave it alone
            settings.resultFileName = projectInfo.projectName;

            if (Config.settings.general.typo.enabled)
            {
                Typo.initialize();
            }

            fileContents = System.IO.File.ReadAllText(projectInfo.entryFile.path);
            List<Util.Token> lexedContent = Lexer.lex(fileContents);

            List<AST.Node> nodes = Parser.getInstance().parse(lexedContent);

            ModuleGen.GenerateModule();

            IRGen.generateIR(nodes);

            IRGen.optimizeIR();

            EXE.compileEXE(settings, true);

        }

        AnsiConsole.MarkupLine("[green]SWO project successfully compiled[/]");
    }

    public static void runProject(RunCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[purple]SWO project running...[/]");

        DirectoryInfo dirInfo = new DirectoryInfo(settings.path);

        FileInfo[] files = dirInfo.GetFiles();

        bool fileFound = false;

        foreach (FileInfo file in files)
        {
            if (file.Name == settings.resultFileName)
            {
                fileFound = true;
                break;
            }
        }

        if (!fileFound)
        {
            throw new ArgumentException("SWO result file not found in project path | It is possible that compilation failed");
        }

        Process process = new Process();
        process.StartInfo.FileName = settings.resultFileName;
        process.StartInfo.Arguments = settings.arguments;
        process.StartInfo.UseShellExecute = true;

        process.Start();

    }
}
