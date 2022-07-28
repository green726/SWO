namespace AST;

public class VariableExpression : Expression
{

    private bool parsingArray = false;
    public bool isArrayIndexRef = false;

    public VariableExpression(Util.Token token, AST.Node? parent = null, bool parentRequired = true) : base(token)
    {
        this.nodeType = NodeType.VariableExpression;
        this.generator = new Generator.VariableExpression(this);

        if (token.value.Contains("[") && token.value.Contains("]"))
        {
            String[] splitStr = token.value.Split("[");

            this.value = splitStr[0];
            this.addChild(new NumberExpression(new Util.Token(Util.TokenType.Int, splitStr[1], token.line, token.column + 1), this));
        }
        value = token.value;
        this.parent = parent;


        // bool exists = false;
        //NOTE: below is commented out b/c i think that LLVm IR will do it for me
        //foreach (VariableAssignment varAss in Parser.globalVarAss)
        // {
        //     if (this.varName == varAss.name)
        //     {
        //         exists = true;
        //         break;
        //     }
        // }
        //
        // if (exists == false)
        // {
        //     throw new ParserException($"Unknown variable referenced {this.varName}", token);
        // }


        if (parent != null)
        {
            parent.addChild(this);
        }
        else if (parentRequired)
        {
            throw new ParserException($"Illegal variable expression {this.value}", token);
        }

    }

    public override void addChild(Util.Token child)
    {
        if (child.value == "[" && !parsingArray)
        {
            this.isArrayIndexRef = true;
            this.parsingArray = true;
            this.codeExcerpt += child.value;
            return;
        }
        else if (child.value == "]" && parsingArray)
        {
            this.parsingArray = false;
            this.codeExcerpt += child.value;
            return;
        }
        else if (!this.parsingArray)
        {
            return;
        }
        base.addChild(child);
    }

    public override void addChild(AST.Node child)
    {
        if (!this.parsingArray)
        {
            throw ParserException.FactoryMethod("An illegal child was added to a variable expression", "remove it", child);
        }
        base.addChild(child);
    }
}
