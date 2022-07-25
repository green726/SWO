namespace AST;

public abstract class Expression : Node
{
    public dynamic? value;

    public Expression(Util.Token token, Node? parent = null) : base(token)
    {
        this.expression = true;
    }

    public Expression(Node node, Node? parent = null) : base(node)
    {
        this.expression = true;
    }
}
