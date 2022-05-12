public abstract class ASTNode
{
    public List<ASTNode> children = new List<ASTNode>();
    public ASTNode? parent;

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
        BuiltinCall
    }
    
    public virtual void addParent(ASTNode parent)
    {
        this.parent = parent;
        if (this.parent != null)
        {
            Parser.nodes.Remove(this);
        }
    }

    public virtual void addChild(ASTNode child)
    {
        children.Add(child);
    }
}
