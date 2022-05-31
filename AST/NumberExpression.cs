public class NumberExpression : ASTNode
{
    public double value;

    public NumberExpression(Util.Token token, ASTNode? parent) : base(token)

    {
        this.nodeType = NodeType.NumberExpression;
        this.value = Double.Parse(token.value);
        this.parent = parent;

        if (parent != null)
        {
            this.parent.addChild(this);
        }
        else
        {
            Parser.nodes.Add(this);
        }
    }

}
