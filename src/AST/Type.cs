namespace AST;

public class Type : AST.Node
{
    public string value;
    public Util.ClassType defaultType;

    public Type(Util.Token token) : base(token)
    {
        this.nodeType = NodeType.TypeAST;
        this.value = token.value;
    }

    public Type(string value, AST.Node parent) : base(parent)
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

