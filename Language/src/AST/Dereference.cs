namespace AST;

public class Dereference : Expression
{
    public Expression actualExpr { get; set; }

    public Dereference(Util.Token token) : base(token)
    {
        this.nodeType = NodeType.Dereference;

        this.generator = new Generator.Dereference(this);
    }

    public Dereference(Util.Token token, AST.Node parent) : base(token)
    {
        this.nodeType = NodeType.Dereference;

        this.generator = new Generator.Dereference(this);

        this.parent = parent;

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

        //BUG: need to actually figure out the type of this
        this.type = actualExpr.type;

        base.addChild(child);
    }
}
