namespace AST;

using System.Collections.Generic;

public abstract class Node
{
    public Generator.Base generator;

    public List<Node> children = new List<Node>();
    public Node? parent = null;

    public int line = 0;
    public int column = 0;
    public int charNum = 0;

    public bool isExpression = false;

    public string codeExcerpt = "";

    public NodeType nodeType = NodeType.Unknown;

    public bool newLineReset = false;

    protected Node(Util.Token token)
    {
        this.charNum = token.charNum;
        this.codeExcerpt = token.value;
        this.line = token.line;
        this.column = token.column;
    }

    protected Node(Node node)
    {
        this.line = node.line;
        this.column = node.column;
        this.charNum = node.charNum;
        this.codeExcerpt = node.codeExcerpt;
    }

    public enum NodeType
    {
        Unknown,
        VariableExpression,
        NumberExpression,
        BinaryExpression,
        VariableAssignment,
        Prototype,
        Function,
        FunctionCall,
        BuiltinCall,
        StringExpression,
        Type,
        IfStatement,
        ElseStatement,
        ForLoop,
        PhiVariable,
        ImportStatement,
        Return
    }

    public virtual void addParent(Node parent)
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

    public virtual void addChild(Node child)
    {
        this.codeExcerpt += child.codeExcerpt;
        children.Add(child);
    }

    public virtual void addChild(Util.Token child)
    {
        this.codeExcerpt += child.value;
    }

    public virtual void removeChild(Node child)
    {
        this.codeExcerpt.Replace(child.codeExcerpt, "");
        children.Remove(child);
    }
}
