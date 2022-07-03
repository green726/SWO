public class TypeAST : ASTNode
{
    public string value;
    public Util.ClassType defaultType;

    public TypeAST(Util.Token token) : base(token)
    {
        this.nodeType = NodeType.TypeAST;
        this.value = token.value;
    }

    public TypeAST(string value, ASTNode parent) : base(parent)
    {
        this.nodeType = NodeType.TypeAST;
        this.value = value;
    }

    private void checkTypes(string value)
    {
        foreach (string type in Parser.typeList)
        {
            if (type == value)
            {
                return;
            }
        }
        throw new ParserException("Unknown type used", this);
    }

}

