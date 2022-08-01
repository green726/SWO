using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Tomlyn;

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
        string[] files = System.IO.Directory.GetFiles(settings.path, "*.hproj");
        string tomlText = System.IO.File.ReadAllText(files[0]);
        projectInfo = Toml.ToModel<ProjectInfo>(tomlText);
        // projectInfo.checkPath();
        projectInfo.setConfig();

        // Console.WriteLine("TOML re-converted from string to model than back to string below");
        // Console.WriteLine(Toml.FromModel(projectInfo));

        Config.initialize(projectInfo.configFilePath);


        settings.resultFileName = projectInfo.projectName;

        if (Config.settings.general.typo.enabled)
        {
            Typo.initialize();
        }

        fileContents = System.IO.File.ReadAllText(projectInfo.entryFile.path);

        List<Util.Token> lexedContent = Lexer.lex(fileContents);

        // foreach (Util.Token token in lexedContent)
        // {
        //     Console.WriteLine(token.type + " " + token.value);
        // }

        List<AST.Node> nodes = Parser.parseForLoop(lexedContent);
        //TODO: write some code in the parser that will (when it hits a using statement) read the text from that file (or package) - lex it, and parse it all inserted into the current point in the ASTNodes list then continue on parsing the main file and eventually codegen that
        ModuleGen.GenerateModule(nodes);

        EXE.compileEXE(settings, true);
    }
}
