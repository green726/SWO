namespace AST;

public class StringExpression : AST.Node
{
    public string value;
    public bool builtInString;

    public StringExpression(Util.Token token, AST.Node? parent = null, bool dontAdd = false, bool builtInString = false) : base(token)
    {
        Parser.checkToken(token, expectedType: Util.TokenType.String);

        this.nodeType = NodeType.StringExpression;
        this.value = token.value;
        this.builtInString = builtInString;

        if (dontAdd == true)
        {
            return;
        }

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
