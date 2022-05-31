public class StringExpression : ASTNode
{
    public string value;

    public StringExpression(Util.Token token, ASTNode? parent = null, bool dontAdd = false) : base(token)
    {
        Parser.checkToken(token, expectedType: Util.TokenType.String);

        this.nodeType = NodeType.StringExpression;
        this.value = token.value;

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
