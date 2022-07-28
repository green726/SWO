namespace AST;

public class VariableExpression : Expression
{

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
        bool exists = false;
        this.parent = parent;

        //NOTE: below is commented out b/c i think that LLVm IR will do it for me
        // foreach (VariableAssignment varAss in Parser.globalVarAss)
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
    public override void addChild(AST.Node child)
    {
        if (child.nodeType != NodeType.NumberExpression)
        {
            // throw ParserException.FactoryMethod();
        }
        base.addChild(child);
    }
}
