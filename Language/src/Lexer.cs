
using System.Text;

public static class Lexer
{
    private static List<Util.Token> tokenList;
    private static string[] binOps = { "+", "-", "*", "/", "==", "<" };
    private static string[] assignmentOps = { "=", "+=", "-=", "*=", "/=", ":=" };
    private static string[] specialChars = { ":", ".", ",", ";", "#" };
    private static string[] modifierChars = { "&", "*" };

    public static string inputStr;

    public static List<Util.Token> lex(string input, Spectre.Console.ProgressTask task = null)
    {
        inputStr = input;
        int line = 1;
        int column = 0;
        int charNum = -1;
        tokenList = new List<Util.Token>();
        char lastChar = new char();
        // char nextChar = new char();
        StringBuilder stringBuilder = new StringBuilder();

        if (task != null)
        {
            task.MaxValue = input.Length;
        }

        int idx = -1;
        foreach (char ch in input)
        {
            idx++;
            if (task != null)
            {
                task.Increment(1);
            }
            charNum++;
            column++;
            bool isFinalChar = input.IndexOf(ch) == input.Length - 1;

            //TODO: handle the arrow op


            if (ch == ' ' || isFinalChar || ch == '\n' || ch == ')' || ch == '}' || specialChars.Contains(ch.ToString()) || ch == ';')
            {
                //NOTE: below will avoid doing anything if you are in a string
                if (lastChar != ' ' && !stringBuilder.ToString().StartsWith("\""))
                {
                    checkStringBuilder(stringBuilder, line, column, charNum);
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
                    tokenList.Add(new Util.Token(Util.TokenType.EOL, ch.ToString(), line, column, charNum));
                    lastChar = ch;
                    stringBuilder = new StringBuilder();
                    line++;
                    column = 0;
                    continue;
                // case ' ':
                //     tokenList.Add(new Util.Token(Util.TokenType.Space, ch.ToString(), line, column, charNum: charNum));
                //     lastChar = ch;
                //     stringBuilder = new StringBuilder();
                //     continue;
                case '(':
                    lexDelimiter(Util.TokenType.DelimiterOpen, stringBuilder, ch, line, column, charNum);
                    lastChar = ch;
                    stringBuilder = new StringBuilder();
                    continue;
                case ')':
                    lexDelimiter(Util.TokenType.DelimiterClose, stringBuilder, ch, line, column, charNum);
                    lastChar = ch;
                    stringBuilder = new StringBuilder();
                    continue;
                case '{':
                    lexDelimiter(Util.TokenType.DelimiterOpen, stringBuilder, ch, line, column, charNum);
                    lastChar = ch;
                    stringBuilder = new StringBuilder();
                    continue;
                case '}':
                    lexDelimiter(Util.TokenType.DelimiterClose, stringBuilder, ch, line, column, charNum);
                    lastChar = ch;
                    stringBuilder = new StringBuilder();
                    continue;
                    // case '[':
                    //     lexDelimiter(Util.TokenType.DelimiterOpen, stringBuilder, ch, line, column, charNum);
                    //     lastChar = ch;
                    //     stringBuilder = new StringBuilder();
                    //     continue;
                    // case ']':
                    //     lexDelimiter(Util.TokenType.DelimiterClose, stringBuilder, ch, line, column, charNum);
                    //     lastChar = ch;
                    //     stringBuilder = new StringBuilder();
                    //     continue;
            }

            if (ch == '"')
            {
                if (stringBuilder.ToString().StartsWith("\""))
                {
                    stringBuilder.Append(ch);
                    DebugConsole.WriteAnsi($"[green]detected end of string and builder val is now: {stringBuilder.ToString()}[/]");
                    tokenList.Add(new Util.Token(Util.TokenType.String, stringBuilder.ToString(), line, column));
                    stringBuilder = new StringBuilder();
                    lastChar = ch;
                    continue;
                }
            }

            //NOTE: below needs to stay if
            if (specialChars.Contains(ch.ToString()))
            {
                tokenList.Add(new Util.Token(Util.TokenType.Special, ch, line, column));
            }
            else if (stringBuilder.ToString().EndsWith("-") && ch == '>')
            {
                handleMultiCharSpecial("->", stringBuilder.ToString(), line, column, charNum);
                stringBuilder = new StringBuilder();
            }
            else if (binOps.Contains(ch.ToString()) && lastChar == ' ' && input[idx + 1] == ' ')
            {
                tokenList.Add(new Util.Token(Util.TokenType.Operator, ch, line, column, charNum));
            }
            //NOTE: handling of modifier chars (ie reference/dereference) - they are only handeled if they are at the start of a word
            else if (modifierChars.Contains(ch.ToString()) && stringBuilder.ToString() == "")
            {
                tokenList.Add(new Util.Token(Util.TokenType.Modifier, ch, line, column));
            }
            else if (ch != ' ' && (int)ch != 13)
            {
                stringBuilder.Append(ch.ToString());
            }
            else
            {
                if (stringBuilder.ToString().StartsWith("\""))
                {
                    stringBuilder.Append(ch.ToString());
                }
                else
                {
                    tokenList.Add(new Util.Token(Util.TokenType.Space, ch, line, column));
                }
            }
            lastChar = ch;
        }

        tokenList.Add(new Util.Token(Util.TokenType.EOF, "", line, column, charNum));
        return tokenList;
    }

    public static void handleMultiCharSpecial(string special, string value, int line, int column, int charNum)
    {
        string removedVal = value.Remove(value.Length - 1);
        tokenList.Add(new Util.Token(Util.TokenType.Keyword, removedVal, line, column));
        tokenList.Add(new Util.Token(Util.TokenType.Special, special, line, column));

    }

    public static void lexDelimiter(Util.TokenType type, StringBuilder builder, char ch, int line, int column, int charNum)
    {
        // if (builder.ToString() == "" && type != Util.TokenType.ParenDelimiterClose && type != Util.TokenType.BrackDelimiterClose && type != Util.TokenType.SquareDelimiterClose && type != Util.TokenType.BrackDelimiterOpen)
        // {
        //     throw new ArgumentException($"Illegal delimeter usage( \"{ch}\" ) at {line}:{column}");
        // }
        if (builder.ToString() != "" && builder.ToString() != " ")
        {
            checkStringBuilder(builder, line, column, charNum);
            // tokenList.Add(new Util.Token(Util.TokenType.Keyword, builder.ToString(), line, column, charNum));
        }
        tokenList.Add(new Util.Token(type, ch.ToString(), line, column, charNum, true));
    }

    public static void checkStringBuilder(StringBuilder stringBuilder, int line, int column, int charNum)
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
            tokenList.Add(new Util.Token(Util.TokenType.Int, stringBuilder.ToString(), line, column, charNum));
        }
        else if (double.TryParse(stringBuilder.ToString(), out double dubResult))
        {
            tokenList.Add(new Util.Token(Util.TokenType.Double, stringBuilder.ToString(), line, column, charNum));
        }
        else if (binOps.Contains(stringBuilder.ToString()))
        {
            tokenList.Add(new Util.Token(Util.TokenType.Operator, stringBuilder.ToString(), line, column, charNum));
        }
        else if (assignmentOps.Contains(stringBuilder.ToString()))
        {
            tokenList.Add(new Util.Token(Util.TokenType.AssignmentOp, stringBuilder.ToString(), line, column, charNum, false));
        }
        // else if (firstChar == '"' && stringBuilder.ToString().EndsWith('"'))
        // {
        //     tokenList.Add(new Util.Token(Util.TokenType.String, stringBuilder.ToString(), line, column, charNum));
        // }
        else if (stringBuilder.ToString() != " " && stringBuilder.ToString() != "" && stringBuilder.ToString() != "\n")
        {
            tokenList.Add(new Util.Token(Util.TokenType.Keyword, stringBuilder.ToString(), line, column, charNum));
        }
        else if (specialChars.Contains(stringBuilder.ToString()))
        {
            tokenList.Add(new Util.Token(Util.TokenType.Special, stringBuilder.ToString(), line, column));
        }

    }


}
