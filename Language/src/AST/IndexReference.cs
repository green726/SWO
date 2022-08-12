namespace AST;
using Spectre.Console;


public class IndexReference : Node
{
    public bool isPointer = false;
    public NumberExpression numExpr;
    public IndexReference(Util.Token token, AST.Node? parent = null) : base(token)
    {
        this.nodeType = NodeType.IndexReference;
        this.generator = new Generator.IndexReference(this);


        this.parent = parent;
        this.parent?.addChild(this);

        if (parent == null)
        {
            throw new Exception();
        }

        if (!this.parent.isExpression)
        {
            throw new Exception();
        }

        if (token.value != "[")
        {
            throw new Exception();
        }
    }

    public override void addChild(Node child)
    {
        this.numExpr = (NumberExpression)child;
    }
}
