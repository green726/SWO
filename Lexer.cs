
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
            if (ch == ' ' || isFinalChar)
            {
                if (lastChar != ' ' || isFinalChar)
                {
                    if (int.TryParse(stringBuilder.ToString(), out int result))
                    {
                        tokenList.Add(new Util.Token(Util.tokenType.number, stringBuilder.ToString(), 0, 0));
                    }
                    else if (operators.Contains(stringBuilder.ToString()))
                    {
                        tokenList.Add(new Util.Token(Util.tokenType._operator, stringBuilder.ToString(), 0, 0));
                    }
                    stringBuilder = new StringBuilder();
                    lastChar = ch;
                    continue;
                }
                else
                {
                    lastChar = ch;
                    continue;
                }
            }
            else if (ch == '\n')
            {
                tokenList.Add(new Util.Token(Util.tokenType.blankLine, ch.ToString(), 0, 0));
                lastChar = ch;
                continue;
            }
            stringBuilder.Append(ch.ToString());
            lastChar = ch;
        }


        return tokenList;
    }

}
