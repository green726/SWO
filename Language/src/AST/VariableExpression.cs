namespace AST;

public class VariableExpression : Expression
{
    public VariableExpression(Util.Token token, AST.Node parent, bool parentRequired = true) : base(token)
    {
        this.nodeType = NodeType.VariableExpression;
        this.generator = new Generator.VariableExpression(this);

        this.value = token.value;

        //TODO: implement the same variable scoping in the parser as the generator has
        DebugConsole.Write(this.value);
        AST.Type originalType = parser.getNamedValueInScope(this.value);
        this.type = originalType;
        DebugConsole.WriteAnsi($"[yellow]original type + {this.type.value}[/]");
        // this.type = new Type("int", this);

        this.parent = parent;
        this.newLineReset = true;

        // if (token.value.Contains("[") && token.value.Contains("]"))
        // {
        //     String[] splitStr = token.value.Split("[");
        //
        //     this.value = splitStr[0];
        //     this.addChild(new NumberExpression(new Util.Token(Util.TokenType.Int, splitStr[1], token.line, token.column + 1), this));
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
        if (this.children.Count() > 0)
        {
            this.children.Last().addChild(child);
            return;
        }
        base.addChild(child);
    }

    public override void addChild(AST.Node child)
    {
        if (child.nodeType != NodeType.VariableExpression && child.nodeType != NodeType.IndexReference && child.nodeType != NodeType.Reference && child.nodeType != NodeType.Dereference)
        {
            throw ParserException.FactoryMethod("An illegal child was added to a variable expression", "remove it", child);
        }

        // if (child.nodeType == NodeType.VariableExpression)
        // {
        //     this.isPointer = true;
        // }

        DebugConsole.Write($"adding child to varExpr with type of " + child.nodeType);
        // if (child.nodeType == NodeType.NumberExpression)
        // {
        //     this.numExpr = (NumberExpression)child;
        //     return;
        // }

        if (children.Count() > 0)
        {
            AST.Node lastChild = children.Last();
            child.parent = lastChild;
            lastChild.addChild(child);
            return;
        }

        base.addChild(child);
    }
}
