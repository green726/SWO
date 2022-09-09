namespace AST;

public class Type : AST.Node
{
    public string value;
    public Util.ClassType defaultType;

    //NOTE: this is the size of an array or similar type
    public int? size = null;
    public bool isArray = false;

    public bool isPointer;
    private bool parsingArray = false;

    public Type(Util.Token token) : base(token)
    {
        this.nodeType = NodeType.Type;
        this.generator = new Generator.Type(this);

        //TODO: add pointer location options
        if (token.value.EndsWith("*"))
        {
            this.isPointer = true;
            token.value.Remove(token.value.Length - 1);
        }

        this.value = token.value;
    }

    public Type(string value, AST.Node parent) : base(parent)
    {
        this.nodeType = NodeType.Type;
        this.generator = new Generator.Type(this);

        if (value.EndsWith("*"))
        {
            this.isPointer = true;
            value.Remove(value.Length - 1);
        }

        this.value = value;
    }

    public override void addChild(Util.Token child)
    {
        if (child.value == "[")
        {
            this.isArray = true;
            this.parsingArray = true;
        }
        else if (parsingArray)
        {
            if (child.value == "]")
            {
                this.parsingArray = false;
            }
            else if (child.type == Util.TokenType.Double || child.type == Util.TokenType.Int)
            {
                this.size = int.Parse(child.value);
            }
            else
            {
                throw new ParserException("Illegal array size declaration in type", child);
            }
        }
        base.addChild(child);
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

    public uint getIntBits()
    {
        if (value.StartsWith("int"))
        {
            string intBits = value.Remove(0, 3);

            if (uint.TryParse(intBits, out uint bits))
            {
                return bits;
            }
        }
        if (value.StartsWith("uint"))
        {
            string intBits = value.Remove(0, 4);

            if (uint.TryParse(intBits, out uint bits))
            {
                return bits;
            }
        }

        throw ParserException.FactoryMethod("A non integer was treated as an integer", "Remove the type", this, this?.parent);
    }

}

