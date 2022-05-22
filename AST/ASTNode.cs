public abstract class ASTNode
{
    public List<ASTNode> children = new List<ASTNode>();
    public ASTNode? parent = null;

    public int line = 0;
    public int column = 0;

    public NodeType nodeType;

    public enum NodeType
    {
        NumberExpression,
        BinaryExpression,
        Prototype,
        Function,
        FunctionCall,
        BuiltinCall,
        String
    }

    public virtual void addParent(ASTNode parent)
    {
        if (this.parent != null)
        {
            this.parent.removeChild(this);
        }
        if (parent != null)
        {
            Parser.nodes.Remove(this);
        }
        this.parent = parent;

    }

    public virtual void addChild(ASTNode child)
    {
        children.Add(child);
    }

    public virtual void removeChild(ASTNode child)
    {
        Console.WriteLine("ASTNode remove child called");
        children.Remove(child);
    }
}
