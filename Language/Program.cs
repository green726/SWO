using System.Diagnostics;
using Spectre.Console;
using Newtonsoft.Json;

public static class SWO
{
    private static string fileContents = "";
    public static bool windows = false;
    public static ProjectInfo projectInfo;

    static void Main(string[] args)
    {
        Util.getSWOInstallPath();

        CLI cli = new CLI(args);
    }

    public static void emitAST(CompileCommandSettings settings)
    {
        DebugConsole.log = settings.debugLogging;

        string[] files = System.IO.Directory.GetFiles(settings.path, "*.sproj");

        if (files.Length == 0)
        {
            throw new ArgumentException("No SWO project files found in current directory");
        }

        // string tomlText = System.IO.File.ReadAllText(files[0]);
        // projectInfo = Toml.ToModel<ProjectInfo>(tomlText);
        string jsonText = System.IO.File.ReadAllText(files[0]);
        projectInfo = JsonConvert.DeserializeObject<ProjectInfo>(jsonText)!;
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

                    List<Parser> parsers = Parser.startParsing(lexedContent, projectInfo.entryFile.nameWithoutExtension, projectInfo.entryFile.path, parseTask);

                    var ASTTask = ctx.AddTask("Serializing the AST");
                    foreach (Parser parser in parsers)
                    {
                        parser.writeAST();
                    }
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

            List<Parser> parsers = Parser.startParsing(lexedContent, projectInfo.entryFile.nameWithoutExtension, projectInfo.entryFile.path);

            foreach (Parser parser in parsers)
            {
                parser.writeAST();
            }
        }

        AnsiConsole.MarkupLine("[green]SWO project successfully compiled[/]");
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

        // string tomlText = System.IO.File.ReadAllText(files[0]);
        // projectInfo = Toml.ToModel<ProjectInfo>(tomlText);
        string jsonText = System.IO.File.ReadAllText(files[0]);
        projectInfo = JsonConvert.DeserializeObject<ProjectInfo>(jsonText)!;
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

                    List<Parser> parsers = Parser.startParsing(lexedContent, projectInfo.entryFile.nameWithoutExtension, parseTask);

                    var moduleTask = ctx.AddTask("Initializing LLVM");
                    List<IRGen> generators = ModuleGen.CreateNewGenerators(parsers, moduleTask);

                    var llvmTask = ctx.AddTask("Compiling to LLVM IR");
                    int i = 0;

                    var passTask = ctx.AddTask("Optimizing the LLVM IR");
                    foreach (IRGen generator in generators)
                    {
                        IRGen.generatorStack.Push(generator);
                        generator.generateIR(parsers[i].nodes);
                        generator.optimizeIR(passTask);
                        i++;
                    }

                    var exeCompileTask = ctx.AddTask("Compiling the LLVM IR to your desired output");
                    List<string> fileNames = new List<string>();
                    foreach (IRGen generator in generators)
                    {
                        fileNames.Add(EXE.compileEXE(settings, generator, exeCompileTask));
                    }
                    EXE.link(settings, fileNames);


                    foreach (Parser parser in parsers)
                    {
                    }
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

            List<Parser> parsers = Parser.startParsing(lexedContent, projectInfo.entryFile.nameWithoutExtension, projectInfo.entryFile.path);

            List<IRGen> generators = ModuleGen.CreateNewGenerators(parsers);

            int i = 0;
            foreach (IRGen generator in generators)
            {
                IRGen.generatorStack.Push(generator);
                generator.generateIR(parsers[i].nodes);
                generator.optimizeIR();
                i++;
            }

            List<string> fileNames = new List<string>();
            foreach (IRGen generator in generators)
            {
                fileNames.Add(EXE.compileEXE(settings, generator));
            }
            EXE.link(settings, fileNames);

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

    public static void serializeParsers(List<Parser> parsers)
    {
        foreach (Parser parser in parsers)
        {
            ASTFile file = new ASTFile(parser) { path = "hi" };

            file.write();
        }
    }
}
