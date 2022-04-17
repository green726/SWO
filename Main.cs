
public class greenlang

{
    private static string fileContents;

    static void Main(string[] args)
    {
        string fileToRead = args.Length != 0 ? args[0] : "/home/green726/coding/greenlang/test.hiss";


        fileContents = System.IO.File.ReadAllText(@$"{fileToRead}");

        List<Util.Token> lexedContent = Lexer.lex(fileContents);

        foreach (Util.Token token in lexedContent)
        {
            Console.WriteLine(token.type + " " + token.value);
        }
        Parser.beginParse(lexedContent);
    }
}
