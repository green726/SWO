using System.Collections.Generic;

public abstract class ASTNode
{
    public List<ASTNode> children = new List<ASTNode>();
    public ASTNode? parent = null;

    public int line = 0;
    public int column = 0;
    public int charNum = 0;

    public string codeExcerpt = "";

    public NodeType nodeType;

    public bool newLineReset = false;

    protected ASTNode(Util.Token token)
    {
        this.charNum = token.charNum;
        this.codeExcerpt = token.value;
        this.line = token.line;
        this.column = token.column;
    }

    protected ASTNode(ASTNode node)
    {
        this.line = node.line;
        this.column = node.column;
        this.charNum = node.charNum;
        this.codeExcerpt = node.codeExcerpt;
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
        IfStatement,
        ElseStatement,
        ForLoop,
        PhiVariable
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
            this.parent = parent;
        }

    }

    public virtual void addChild(ASTNode child)
    {
        this.codeExcerpt += child.codeExcerpt;
        children.Add(child);
    }

    public virtual void addChild(Util.Token child)
    {
        this.codeExcerpt += child.value;
    }

    public virtual void removeChild(ASTNode child)
    {
        this.codeExcerpt.Replace(child.codeExcerpt, "");
        children.Remove(child);
    }
}
