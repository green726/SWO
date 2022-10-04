namespace AST;

public class Reference : Expression
{
    public Expression actualExpr { get; set; }

    public Reference(Util.Token token, AST.Node? parent = null) : base(token)
    {
        this.nodeType = NodeType.Reference;

        this.generator = new Generator.Reference(this);

        this.parent = parent;

        parent?.addChild(this);
    }

    public override void addChild(Node child)
    {
        //TODO: maybe implement this to allow types to have a "*" at the beginning to mark a ptr type
        // if (child.nodeType == AST.Node.NodeType.Type)
        // {
        //     Type type = (Type)child;
        //     type.isPointer = true;
        // }
        if (!child.isExpression)
        {
            throw ParserException.FactoryMethod("A non expression was referenced", "Remove the reference (\"&\")", child, this);
        }

        this.actualExpr = (AST.Expression)child;
        this.actualExpr.isReference = true;

        Parser.getInstance().parent = this.parent;

        base.addChild(child);
    }
}
