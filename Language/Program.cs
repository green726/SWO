using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class HISS
{
    private static string fileContents;
    private static bool windows = false;

    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "test")
        {
            testFunc();
            return;
        }
        CLI cli = new CLI(args);


    }

    public static void testFunc()
    {
        string pathToSrc = "";




        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            pathToSrc = "/home/green726/coding/HISS/src/";
            Config.initialize("~/.HISS/Resources/Config/config.toml");
        }

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            pathToSrc = @"D:\coding\HISS\src\";
            windows = true;
            Config.initialize(Environment.ExpandEnvironmentVariables(@"%UserProfile%\.HISS\Resources\Config\config.toml"));
        }


        if (Config.settings.general.typo.enabled)
        {
            Typo.initialize();
        }

        string fileToRead = pathToSrc + "/test.hiss";

        string debugLoggingStr = "true";
        bool debugLogging = debugLoggingStr == "true" ? true : false;

        fileContents = System.IO.File.ReadAllText(@$"{fileToRead}");

        List<Util.Token> lexedContent = Lexer.lex(fileContents);

        foreach (Util.Token token in lexedContent)
        {
            Console.WriteLine(token.type + " " + token.value);
        }

        List<AST.Node> nodes = Parser.beginParse(lexedContent);
        ModuleGen.GenerateModule(nodes);


        EXE.compileEXE(windows);
    }

    public static void compileProject(CompileCommandSettings settings, ProjectInfo info)
    {
        Config.initialize(info.configFilePath);

        if (Config.settings.general.typo.enabled)
        {
            Typo.initialize();
        }

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            windows = true;
        }

        fileContents = System.IO.File.ReadAllText(info.entryFile.path);

        List<Util.Token> lexedContent = Lexer.lex(fileContents);

        // foreach (Util.Token token in lexedContent)
        // {
        //     Console.WriteLine(token.type + " " + token.value);
        // }

        List<AST.Node> nodes = Parser.beginParse(lexedContent);
        //TODO: write some code in the parser that will (when it hits a using statement) read the text from that file (or package) - lex it, and parse it all inserted into the current point in the ASTNodes list then continue on parsing the main file and eventually codegen that
        ModuleGen.GenerateModule(nodes);

        EXE.compileEXE(windows);
    }
}
