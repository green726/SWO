using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Tomlyn;
using Spectre.Console;

public static class HISS
{
    private static string fileContents;
    public static bool windows = false;
    public static ProjectInfo projectInfo;

    static void Main(string[] args)
    {
        Util.getHISSInstallPath();
        if (args.Length > 0 && args[0] == "test")
        {
            testFunc();
            return;
        }
        CLI cli = new CLI(args);


    }

    public static void testFunc()
    {
        if (Config.settings.general.typo.enabled)
        {
            Typo.initialize();
        }

        string fileToRead = Util.pathToSrc + "/test.hiss";

        string debugLoggingStr = "true";
        bool debugLogging = debugLoggingStr == "true" ? true : false;

        fileContents = System.IO.File.ReadAllText(@$"{fileToRead}");

        List<Util.Token> lexedContent = Lexer.lex(fileContents);

        foreach (Util.Token token in lexedContent)
        {
            Console.WriteLine(token.type + " " + token.value);
        }

        List<AST.Node> nodes = Parser.parseForLoop(lexedContent);
        ModuleGen.GenerateModule(nodes);


        EXE.compileEXE(windows);
    }

    public static void compileProject(CompileCommandSettings settings)
    {
        Console.WriteLine(settings.resultFileType);
        string[] files = System.IO.Directory.GetFiles(settings.path, "*.hproj");
        string tomlText = System.IO.File.ReadAllText(files[0]);
        projectInfo = Toml.ToModel<ProjectInfo>(tomlText);
        // projectInfo.checkPath();
        projectInfo.setConfig();

        Config.initialize(projectInfo.configFilePath);

        settings.resultFileName = projectInfo.projectName;

        if (Config.settings.general.typo.enabled)
        {
            Typo.initialize();
        }

        fileContents = System.IO.File.ReadAllText(projectInfo.entryFile.path);

        List<Util.Token> lexedContent = Lexer.lex(fileContents);

        List<AST.Node> nodes = Parser.parseForLoop(lexedContent);

        ModuleGen.GenerateModule(nodes);

        EXE.compileEXE(settings, true);

        AnsiConsole.MarkupLine("[green]SWO project successfully compiled[/]");
    }

    public static void runProject(RunCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[purple]SWO project running...[/]");

        DirectoryInfo dirInfo = new DirectoryInfo(settings.compileCommandSettings.path);

        FileInfo[] files = dirInfo.GetFiles();

        bool fileFound = false;

        foreach (FileInfo file in files)
        {
            if (file.Name == settings.compileCommandSettings.resultFileName)
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
        process.StartInfo.FileName = settings.compileCommandSettings.resultFileName;
        process.StartInfo.Arguments = settings.arguments;
        process.StartInfo.UseShellExecute = true;

        process.Start();

    }
}
