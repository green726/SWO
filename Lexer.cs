
public static class Lexer
{
    public enum tokenType
    {
        _operator,
        number
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
    private static char[] operators = { '+', '-', '*', '/' };

    public static List<Token> lex(string input)
    {
        tokenList = new List<Token>();

        foreach (char ch in input)
        {
            if (ch == ' ')
            {
                continue;
            }

            else if (Char.IsDigit(ch))
                tokenList.Add(new Token(tokenType.number, ch));
            else if (operators.Contains(ch))
                tokenList.Add(new Token(tokenType._operator, ch));
        }

        return tokenList;
    }

}
