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

    public Type containedType = null;

    public Type(Util.Token token) : base(token)
    {
        //TODO: check if the parsers next non space is a bracket and if so advance the parser token and incoroprate the array into this things token
        this.nodeType = NodeType.Type;
        this.generator = new Generator.Type(this);

        if (parser.nextNonSpace().value == "[")
        {
            for (int i = 1; i < 4; i++)
            {
                token.value += parser.tokenList[parser.currentTokenNum + i].value;
            }
            //NOTE: adds 3 to skip the rest of the array (minus one b/c the parser will also increment)
            /*
                0 = int
                1 = [
                2 = 12301230
                3 = ]
                4 = akdsjaksdjjashsdkjahsjd
            */
            parser.currentTokenNum += 3;
        }

        this.value = token.value;

        if (token.value.EndsWith("*"))
        {
            this.isPointer = true;
            this.containedType = new Type(token.value.Substring(0, token.value.Length - 1), this);
        }
        else if (token.value.Contains("[") && token.value.IndexOf("]") > token.value.IndexOf("["))
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
            this.containedType = new Type(token.value.Substring(0, idxFirstBrack), this);
        }

        if (this.containedType != null)
        {
            this.isStruct = this.containedType.isStruct;
            this.isTrait = this.containedType.isTrait;
        }
        else
        {
            (this.isStruct, this.isTrait) = TypeInformation.checkForCustomType(this.value, parser);
        }
    }

    public Type(string value, AST.Node parent) : base(parent)
    {
        this.nodeType = NodeType.Type;
        this.generator = new Generator.Type(this);


        if (parser.nextNonSpace().value == "[")
        {
            for (int i = 1; i < 4; i++)
            {
                value += parser.tokenList[parser.currentTokenNum + i].value;
            }
            //NOTE: adds 3 to skip the rest of the array (minus one b/c the parser will also increment)
            /*
                0 = int
                1 = [
                2 = 12301230
                3 = ]
                4 = akdsjaksdjjashsdkjahsjd
            */
            parser.currentTokenNum += 3;
        }

        this.value = value;

        if (value.EndsWith("*"))
        {
            this.isPointer = true;
            this.containedType = new Type(value.Substring(0, value.Length - 1), this);
        }
        else if (value.Contains("[") && value.IndexOf("]") > value.IndexOf("["))
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
            this.containedType = new Type(value.Substring(0, idxFirstBrack), this);
        }

        if (this.containedType != null)
        {
            this.isStruct = this.containedType.isStruct;
            this.isTrait = this.containedType.isTrait;
        }
        else
        {
            (this.isStruct, this.isTrait) = TypeInformation.checkForCustomType(this.value, parser);
        }
    }

    public string getContainedTypeString(AST.Node caller)
    {
        if (this.containedType == null)
        {
            throw ParserException.FactoryMethod("Attempted to get the contained type of a non-array / non-pointer type", "Internal compiler error - make an issue on GitHub", caller, this);
        }
        return this.containedType.value;
    }

    public override void addChild(Util.Token child)
    {
        base.addChild(child);
    }
}

