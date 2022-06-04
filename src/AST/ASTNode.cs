using System.Collections.Generic;

public abstract class ASTNode
{
    public List<ASTNode> children = new List<ASTNode>();
    public ASTNode? parent = null;

    public int line = 0;
    public int column = 0;

    public NodeType nodeType;

    protected ASTNode(Util.Token token)
    {
        this.line = token.line;
        this.column = token.column;
    }

    protected ASTNode(ASTNode node)
    {
        this.line = node.line;
        this.column = node.column;
    }

    public enum NodeType
    {
        VariableExpression,
        NumberExpression,
        BinaryExpression,
        VariableAssignment,
        Prototype,
        Function,
        FunctionCall,
        BuiltinCall,
        StringExpression,
        TypeAST,
        IfStatement
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

    public virtual void addChild(Util.Token child)
    {
    }

    public virtual void removeChild(ASTNode child)
    {
        children.Remove(child);
    }
}
