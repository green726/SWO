namespace AST;

public class Type : AST.Node
{
    public string value;

    //NOTE: this is the size of an array or similar type
    public int size = 0;
    public bool isArray = false;

    public bool isStruct = false;
    public bool isTrait = false;

    public bool isPointer;
    private bool parsingArray = false;

    public Type(Util.Token token) : base(token)
    {
        this.nodeType = NodeType.Type;
        this.generator = new Generator.Type(this);

        if (token.value.EndsWith("*"))
        {
            DebugConsole.WriteAnsi("[red] detected pointer type(pre)[/]" + token.value);
            this.isPointer = true;
            token.value = token.value.Substring(0, token.value.Length - 1);
            DebugConsole.WriteAnsi("[red] detected pointer type(post)[/]" + token.value);
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
        (this.isStruct, this.isTrait) = TypeInformation.checkForCustomType(this.value, parser);
    }

    public Type(string value, AST.Node parent) : base(parent)
    {
        this.nodeType = NodeType.Type;
        this.generator = new Generator.Type(this);

        if (value.EndsWith("*"))
        {
            DebugConsole.WriteAnsi("[red] detected pointer type(pre)[/]" + value);
            this.isPointer = true;
            value = value.Substring(0, value.Length - 1);
            DebugConsole.WriteAnsi("[red] detected pointer type(post)[/]" + value);
        }

        if (value.Contains("[") && value.IndexOf("]") > value.IndexOf("["))
        {
            this.isArray = true;
            //handles array types
            int idxFirstBrack = value.IndexOf("[");
            int idxSecondBrack = value.IndexOf("]");

            if (idxFirstBrack + 1 == idxSecondBrack)
            {
                // TODO: implement auto-array sizing (gonna need to do it based on future values somehow)
            }
            else
            {
                string arrSizeStr = "";
                foreach (char ch in value.Substring(idxFirstBrack + 1, idxSecondBrack - (idxFirstBrack + 1)))
                {
                    if (!Char.IsDigit(ch))
                    {
                        throw ParserException.FactoryMethod($"Illegal non-integer in array size declaration({ch})", "Replace it with an integer", this.parent);
                    }
                    arrSizeStr += ch;
                }
                this.size = int.Parse(arrSizeStr);
            }
        }

        this.value = value;
        (this.isStruct, this.isTrait) = TypeInformation.checkForCustomType(this.value, parser);
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
}

