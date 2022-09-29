namespace AST;

public class Dereference : Expression
{
    public Expression actualExpr { get; set; }

    public Dereference(Util.Token token, AST.Node? parent = null) : base(token)
    {
        this.nodeType = NodeType.Dereference;

        this.generator = new Generator.Dereference(this);

        parent?.addChild(this);
    }

    public override void addChild(Node child)
    {
        if (!child.isExpression)
        {
            throw ParserException.FactoryMethod("A non expression was referenced", "Remove the reference (\"&\")", child, this);
        }

        this.actualExpr = (AST.Expression)child;
        this.actualExpr.isDereference = true;

        base.addChild(child);
    }
}
