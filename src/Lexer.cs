
using System.Text;

public static class Lexer
{
    private static List<Util.Token> tokenList;
    private static string[] binOps = { "+", "-", "*", "/", "==" };
    private static string[] assignmentOps = { "=", "+=", "-=", "*=", "/=" };

    public static List<Util.Token> lex(string input)
    {
        int line = 1;
        int column = 0;
        int charNum = 0;
        tokenList = new List<Util.Token>();
        char lastChar = new char();
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char ch in input)
        {
            charNum++;
            column++;
            bool isFinalChar = input.IndexOf(ch) == input.Length - 1;
            if (ch == ' ' || isFinalChar || ch == '\n' || ch == ')' || ch == '}'/*  || lastChar == '\n' */)
            {
                if (lastChar != ' ' && lastChar != null)
                {
                    char firstChar = ' ';
                    try
                    {
                        firstChar = stringBuilder.ToString()[0];
                    }
                    catch
                    { }
                    if (int.TryParse(stringBuilder.ToString(), out int result))
                    {
                        tokenList.Add(new Util.Token(Util.TokenType.Number, stringBuilder.ToString(), line, column));
                    }
                    else if (binOps.Contains(stringBuilder.ToString()))
                    {
                        tokenList.Add(new Util.Token(Util.TokenType.Operator, stringBuilder.ToString(), line, column));
                    }
                    else if (assignmentOps.Contains(stringBuilder.ToString()))
                    {
                        tokenList.Add(new Util.Token(Util.TokenType.AssignmentOp, stringBuilder.ToString(), line, column, false));
                    }
                    else if (firstChar == '"' && stringBuilder.ToString().EndsWith('"'))
                    {
                        tokenList.Add(new Util.Token(Util.TokenType.String, stringBuilder.ToString(), line, column));
                    }
                    else if (stringBuilder.ToString() != " " && stringBuilder.ToString() != "" && stringBuilder.ToString() != "\n")
                    {
                        tokenList.Add(new Util.Token(Util.TokenType.Keyword, stringBuilder.ToString(), line, column));
                    }
                    stringBuilder = new StringBuilder();
                    lastChar = ch;
                }
                else
                {
                }
            }
            switch (ch)
            {
                case '\n':
                    tokenList.Add(new Util.Token(Util.TokenType.EOL, ch.ToString(), line, column));
                    lastChar = ch;
                    stringBuilder = new StringBuilder();
                    line++;
                    column = 0;
                    continue;
                case '(':
                    lexDelimiter(Util.TokenType.ParenDelimiterOpen, stringBuilder, ch, line, column);
                    lastChar = ch;
                    stringBuilder = new StringBuilder();
                    continue;
                case ')':
                    lexDelimiter(Util.TokenType.ParenDelimiterClose, stringBuilder, ch, line, column);
                    lastChar = ch;
                    stringBuilder = new StringBuilder();
                    continue;
                case '{':
                    lexDelimiter(Util.TokenType.BrackDelimiterOpen, stringBuilder, ch, line, column);
                    lastChar = ch;
                    stringBuilder = new StringBuilder();
                    continue;
                case '}':
                    lexDelimiter(Util.TokenType.BrackDelimiterClose, stringBuilder, ch, line, column);
                    lastChar = ch;
                    stringBuilder = new StringBuilder();
                    continue;
                case '[':
                    lexDelimiter(Util.TokenType.SquareDelimiterOpen, stringBuilder, ch, line, column);
                    lastChar = ch;
                    stringBuilder = new StringBuilder();
                    continue;
                case ']':
                    lexDelimiter(Util.TokenType.SquareDelimiterClose, stringBuilder, ch, line, column);
                    lastChar = ch;
                    stringBuilder = new StringBuilder();
                    continue;
            }

            if (ch != ' ' && ch.ToString() != null && (int)ch != 13)
            {
                // Console.WriteLine($"Appending char: {ch} with code: {(int)ch} and char num of {charNum}");
                stringBuilder.Append(ch.ToString());
            }
            lastChar = ch;
        }

        tokenList.Add(new Util.Token(Util.TokenType.EOF, "", line, column));
        return tokenList;
    }

    public static void lexDelimiter(Util.TokenType type, StringBuilder builder, char ch, int line, int column)
    {
        // if (builder.ToString() == "" && type != Util.TokenType.ParenDelimiterClose && type != Util.TokenType.BrackDelimiterClose && type != Util.TokenType.SquareDelimiterClose && type != Util.TokenType.BrackDelimiterOpen)
        // {
        //     throw new ArgumentException($"Illegal delimeter usage( \"{ch}\" ) at {line}:{column}");
        // }
        if (builder.ToString() != "" && builder.ToString() != " ") tokenList.Add(new Util.Token(Util.TokenType.Keyword, builder.ToString(), line, column));
        tokenList.Add(new Util.Token(type, ch.ToString(), line, column, true));
    }


}
