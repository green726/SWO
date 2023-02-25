namespace AST;

public class Reference : Expression
{
    public Expression actualExpr { get; set; }

    public Reference(Util.Token token, AST.Node parent) : base(token)
    {
        this.nodeType = NodeType.Reference;

        this.generator = new Generator.Reference(this);

        this.parent = parent;

        parent?.addChild(this);
    }

    public override void addChild(Node child)
    {
        base.addChild(child);
        if (!child.isExpression)
        {
            throw ParserException.FactoryMethod("A non expression was referenced", "Remove the reference (\"&\")", child, this);
        }
        else if (this.actualExpr != null)
        {
            throw ParserException.FactoryMethod("A reference was already set", "Remove the reference (\"&\")", child, this);
        }


        this.actualExpr = (AST.Expression)child;
        this.actualExpr.isReference = true;


        this.type = new ParserTypeInformation("*" + actualExpr.type.value, this.parser);
        DebugConsole.Write(this.type.value);
    }
}
