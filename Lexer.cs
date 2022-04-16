
using System.Text;

public static class Lexer
{
    public enum tokenType
    {
        _operator,
        number,
        blankLine
    }


    public class Token
    {
        public string value;
        public tokenType type;

        public Token(tokenType type, string value)
        {
            this.value = value;
            this.type = type;
        }

        public Token(tokenType type, char value)
        {
            this.value = value.ToString();
            this.type = type;
        }

    }
    private static List<Token> tokenList;
    private static string[] operators = { "+", "-", "*", "/" };

    public static List<Token> lex(string input)
    {
        tokenList = new List<Token>();
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
                        tokenList.Add(new Token(tokenType.number, stringBuilder.ToString()));
                    }
                    else if (operators.Contains(stringBuilder.ToString()))
                    {
                        tokenList.Add(new Token(tokenType._operator, stringBuilder.ToString()));
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
                tokenList.Add(new Token(tokenType.blankLine, ch.ToString()));
                lastChar = ch;
                continue;
            }
            stringBuilder.Append(ch.ToString());
            lastChar = ch;
        }


        return tokenList;
    }

}
