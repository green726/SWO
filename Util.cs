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
 


}
