using System.Runtime.InteropServices;

public class greenlang
{
    private static string fileContents;

    static void Main(string[] args)
    {

        string fileToRead = args.Length != 0 ? args[0] : "/coding/HISS/test.hiss";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            fileToRead = args.Length != 0 ? args[0] : "/home/green726/coding/greenlang/test.hiss";
        }


        string debugLoggingStr = args.Length != 0 ? args[1] : "true";
        bool debugLogging = debugLoggingStr == "true" ? true : false;

        fileContents = System.IO.File.ReadAllText(@$"{fileToRead}");

        List<Util.Token> lexedContent = Lexer.lex(fileContents);

        foreach (Util.Token token in lexedContent)
        {
            Console.WriteLine(token.type + " " + token.value);
        }

        List<ASTNode> nodes = Parser.beginParse(lexedContent);
        ModuleGen.GenerateModule(nodes);


        EXE.compileEXE();
    }
}
