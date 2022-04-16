
public class greenlang

{
    private static string fileContents;

    static void Main(string[] args)
    {
        string fileToRead = args[0];

        fileContents = System.IO.File.ReadAllText(@$"{fileToRead}");

        List<Lexer.Token> lexedContent = Lexer.lex(fileContents);


        foreach (Lexer.Token token in lexedContent)
        {
            Console.WriteLine(token.type + " " + token.value);
        }
    }
}
