namespace AST;
using Spectre.Console;


public class IndexReference : Node
{
    public NumberExpression numExpr;
    public bool isPointer = false;

    public IndexReference(Util.Token token, AST.Node parent) : base(token, parent)
    {
        this.nodeType = NodeType.IndexReference;
        this.generator = new Generator.IndexReference(this);

        this.parent = parent;
        this.parent?.addChild(this);

        if (parent.nodeType == AST.Node.NodeType.Empty)
        {
            throw new Exception();
        }

        if (!this.parent.isExpression)
        {
            throw new Exception();
        }
    }
}
