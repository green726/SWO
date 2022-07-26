namespace AST;

public abstract class Expression : Node
{
    public dynamic? value { get; set; }

    public Expression(Util.Token token, Node? parent = null) : base(token)
    {
        this.isExpression = true;
    }

    public Expression(Node node, Node? parent = null) : base(node)
    {
        this.isExpression = true;
    }
}
