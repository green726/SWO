
using System.Text;

public static class Lexer
{
    private static List<Util.Token> tokenList;
    private static string[] operators = { "+", "-", "*", "/" };

    public static List<Util.Token> lex(string input)
    {
        tokenList = new List<Util.Token>();
        char lastChar = new char();
        StringBuilder stringBuilder = new StringBuilder();


        foreach (char ch in input)
        {
            bool isFinalChar = input.IndexOf(ch) == input.Length - 1;
            if (ch == ' ' || isFinalChar || lastChar == ')' || lastChar == '}' || lastChar == '\n')
            {
                if (lastChar != ' ')
                {
                    if (int.TryParse(stringBuilder.ToString(), out int result))
                    {
                        tokenList.Add(new Util.Token(Util.TokenType.Number, stringBuilder.ToString(), 0, 0));
                    }
                    else if (operators.Contains(stringBuilder.ToString()))
                    {
                        tokenList.Add(new Util.Token(Util.TokenType.Operator, stringBuilder.ToString(), 0, 0));
                    }
                    else
                    {
                        tokenList.Add(new Util.Token(Util.TokenType.Keyword, stringBuilder.ToString(), 0, 0));
                    }
                    stringBuilder = new StringBuilder();
                    lastChar = ch;
                }
                else
                {
                    lastChar = ch;
                    continue;
                }
            }
            switch (ch)
            {
                case '\n':
                    tokenList.Add(new Util.Token(Util.TokenType.EOL, ch.ToString(), 0, 0));
                    lastChar = ch;
                    continue;
                case '(':
                    lexDelimiter(Util.TokenType.ParenDelimiterOpen, stringBuilder, ch, 0, 0);
                    lastChar = ch;
                    continue;
                case ')':
                    lexDelimiter(Util.TokenType.ParenDelimiterClose, stringBuilder, ch, 0, 0);
                    lastChar = ch;
                    continue;
                case '{':
                    lexDelimiter(Util.TokenType.BrackDelimiterOpen, stringBuilder, ch, 0, 0);
                    lastChar = ch;
                    continue;
                case '}':
                    lexDelimiter(Util.TokenType.BrackDelimiterClose, stringBuilder, ch, 0, 0);
                    lastChar = ch;
                    continue;
            }

            if (ch != ' ')
            {
                stringBuilder.Append(ch.ToString());
            }
            lastChar = ch;
        }

        tokenList.Add(new Util.Token(Util.TokenType.EOF, "", 0, 0));
        return tokenList;
    }

    public static void lexDelimiter(Util.TokenType type, StringBuilder builder, char ch, int column, int line)
    {
        if (builder.ToString() != "")
        {
            throw new ArgumentException($"Illegal delimeter usage at {line}:{column}");
        }
        tokenList.Add(new Util.Token(type, ch.ToString(), 0, 0));
    }

}
