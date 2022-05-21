public class StringExpression : ASTNode
{
    public string value;

    public StringExpression(Util.Token token)
    {
        Parser.checkToken(token, expectedType: Util.TokenType.Keyword);
    
        this.nodeType = NodeType.String;
        this.value = token.value;

        this.line = token.line;
        this.column = token.column;
    }

}
