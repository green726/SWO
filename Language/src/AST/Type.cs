namespace AST;

public class Type : AST.Node
{
    public string value;
    public Util.ClassType defaultType;

    //NOTE: this is the size of an array or similar type
    public int? size = null;
    public bool isArray = false;


    public Type(Util.Token token) : base(token)
    {
        this.nodeType = NodeType.Type;
        this.generator = new Generator.Type(this);

        this.value = token.value;
    }

    public Type(string value, AST.Node parent) : base(parent)
    {
        this.nodeType = NodeType.Type;
        this.generator = new Generator.Type(this);

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

