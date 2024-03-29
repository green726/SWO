namespace AST;

public class StringExpression : Expression
{
    // public new string value;
    public bool builtInString;

    public StringExpression(Util.Token token, AST.Node parent, bool dontAdd = false, bool builtInString = false) : base(token)
    {
        this.nodeType = NodeType.StringExpression;
        this.generator = new Generator.StringExpression(this);

        parser.checkToken(token, expectedType: Util.TokenType.String);

        string valueWithoutQuotes = token.value.Substring(1, token.value.Length - 2);
        this.value = valueWithoutQuotes;
        this.builtInString = builtInString;

        this.type = new ParserTypeInformation("string");

        if (dontAdd == true)
        {
            return;
        }

        DebugConsole.WriteAnsi($"[red]string expr with value of: {this.value}[/]");
        if (parent != null)
        {
            parent.addChild(this);
        }
        else
        {
            throw new ParserException("Illegal string expression", token);
        }

    }

}
