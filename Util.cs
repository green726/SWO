public static class Util
{
    public enum TokenType
    {
        Operator,
        Number,
        BlankLine,
        EOF
    }


    public class Token
    {
        public string value;
        public TokenType type;
        public int line;
        public int column;

        public Token(TokenType type, string value, int line, int column)
        {
            this.value = value;
            this.type = type;
            this.line = line;
            this.column = column;
        }

        public Token(TokenType type, char value, int line, int column)
        {
            this.value = value.ToString();
            this.type = type;
            this.line = line;
            this.column = column;
        }

    }



}
