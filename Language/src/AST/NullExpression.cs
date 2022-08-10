namespace AST;

public class NullExpression : Expression
{
    public NullExpression(Util.Token token, Node? parent = null) : base(token, parent)
    {
        this.nodeType = AST.Node.NodeType.NullExpression;
        this.generator = new Generator.NullExpression(this);

        this.parent = parent;
        this.parent?.addChild(this);
    }
}
