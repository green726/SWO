public static class Util
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
        public int line;
        public int column;

        public Token(tokenType type, string value, int line, int column)
        {
            this.value = value;
            this.type = type;
            this.line = line;
            this.column = column;
        }

        public Token(tokenType type, char value, int line, int column)
        {
            this.value = value.ToString();
            this.type = type;
            this.line = line;
            this.column = column;
        }

    }



}
