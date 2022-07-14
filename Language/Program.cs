using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class HISS
{
    private static string fileContents;
    private static bool windows = false;

    static void Main(string[] args)
    {
        Typo.initialize();
        Config.initialize();

        CLI.parseOptions(args);

        string fileToRead = /* args.Length != 0 ? args[0] : */ "/coding/HISS/src/test.hiss";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            fileToRead = /* args.Length != 0 ? args[0] : */ "/home/green726/coding/HISS/src/test.hiss";
        }

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            windows = true;
        }

        string debugLoggingStr = /* args.Length != 0 ? args[1] : */ "true";
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
}
