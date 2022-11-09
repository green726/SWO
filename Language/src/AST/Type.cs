namespace AST;

public class Type : AST.Node
{
    public string value;

    //NOTE: this is the size of an array or similar type
    public int size = 0;
    public bool isArray = false;

    public bool isPointer;
    private bool parsingArray = false;

    public Type(Util.Token token) : base(token)
    {
        this.nodeType = NodeType.Type;
        this.generator = new Generator.Type(this);

        if (token.value.EndsWith("*"))
        {
            this.isPointer = true;
            token.value = token.value.Substring(0, token.value.Length - 1);
        }

        if (token.value.Contains("[") && token.value.IndexOf("]") > token.value.IndexOf("["))
        {
            this.isArray = true;
            //handles array types
            int idxFirstBrack = token.value.IndexOf("[");
            int idxSecondBrack = token.value.IndexOf("]");

            if (idxFirstBrack + 1 == idxSecondBrack)
            {
                // TODO: implement auto-array sizing (gonna need to do it based on future values somehow)
            }
            else
            {
                string arrSizeStr = "";
                foreach (char ch in token.value.Substring(idxFirstBrack + 1, idxSecondBrack - (idxFirstBrack + 1)))
                {
                    if (!Char.IsDigit(ch))
                    {
                        throw ParserException.FactoryMethod($"Illegal non-integer in array size declaration({ch})", "Replace it with an integer", token, this.parent);
                    }
                    arrSizeStr += ch;
                }
                this.size = int.Parse(arrSizeStr);
            }
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

    public Type getContainedType(AST.Node caller)
    {
        if (!this.isArray)
        {
            throw ParserException.FactoryMethod("Attempted to get the contained type of a non-array", "Internal compiler error - make an issue on GitHub", caller, this);
        }
        Util.Token typeTok = new Util.Token(Util.TokenType.Keyword, this.value.Remove(this.value.IndexOf("[")), this.line, this.column, false);
        return new Type(typeTok);
    }

    public string getContainedTypeString(AST.Node caller)
    {
        if (!this.isArray)
        {
            throw ParserException.FactoryMethod("Attempted to get the contained type of a non-array", "Internal compiler error - make an issue on GitHub", caller, this);
        }
        return this.value.Remove(this.value.IndexOf("["));
    }


    public override void addChild(Util.Token child)
    {
        base.addChild(child);
    }

    private void checkTypes(string value)
    {
        foreach (string type in parser.typeList)
        {
            if (type == value)
            {
                return;
            }
        }
        throw new ParserException("Unknown type used", this);
    }
}

