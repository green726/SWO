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

        DebugConsole.Write("actual expr node type: " + this.actualExpr.nodeType);
        DebugConsole.Write("actual expr: " + this.actualExpr.value);
        DebugConsole.Write("actual expr type: " + this.actualExpr.type.value);
        //BUG: need to actually figure out the type of this
        this.type = (ParserTypeInformation)actualExpr.type;
        DebugConsole.Write("type: " + this.type.value);
        this.value = "*" + this.actualExpr.value;

        base.addChild(child);
    }
}
