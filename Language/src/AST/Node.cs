namespace AST;

using System.Collections.Generic;
using Newtonsoft.Json;

public abstract class Node
{
    [JsonIgnore]
    public Generator.Base generator;

    [JsonIgnore]
    public Parser parser;

    [JsonIgnore]
    private Parser parentParser;

    public List<Node> children = new List<Node>();
    [JsonIgnore]
    public Node parent;

    public int line = 0;
    public int column = 0;
    public int charNum = 0;

    public bool isExpression = false;

    public string codeExcerpt = "";

    public NodeType nodeType = NodeType.Unknown;

    public bool newLineReset = false;

    public bool exportChecked = false;

    protected Node()
    {
        this.parent = this;
        this.charNum = 0;
        this.codeExcerpt = "";
        this.line = 0;
        this.column = 0;
        this.parser = Parser.getInstance();
        this.parentParser = parser?.parentParser;
        this.generator = new Generator.Empty(this);
        // this.parser.previousNode = this;
    }

    protected Node(Util.Token token)
    {
        this.charNum = token.charNum;
        this.codeExcerpt = token.value;
        this.line = token.line;
        this.column = token.column;
        this.parser = Parser.getInstance();
        this.parentParser = parser.parentParser;
        this.generator = new Generator.Empty(this);
        this.parent = new Empty();
        // this.parser.previousNode = this;
    }

    protected Node(Util.Token token, Node parent)
    {
        this.charNum = token.charNum;
        this.codeExcerpt = token.value;
        this.line = token.line;
        this.column = token.column;
        this.parser = Parser.getInstance();
        this.parentParser = parser.parentParser;
        this.generator = new Generator.Empty(this);
        this.parent = parent;
        // this.parent.addChild(this);
        // this.parser.previousNode = this;
    }

    protected Node(Node node, Node parent)
    {
        this.line = node.line;
        this.column = node.column;
        this.charNum = node.charNum;
        this.codeExcerpt = node.codeExcerpt;
        this.parser = Parser.getInstance();
        this.parentParser = parser.parentParser;
        this.generator = new Generator.Empty(this);
        this.parent = parent;
        this.parent.addChild(this);
        // this.parser.previousNode = this;
    }

    protected Node(Node node, bool useNodeParser)
    {
        this.line = node.line;
        this.column = node.column;
        this.charNum = node.charNum;
        this.codeExcerpt = node.codeExcerpt;
        this.parser = node.parser;
        this.parentParser = parser.parentParser;
        this.generator = new Generator.Empty(this);
        this.parent = new Empty();
        // this.parser.previousNode = this;
    }


    protected Node(Node node)
    {
        this.line = node.line;
        this.column = node.column;
        this.charNum = node.charNum;
        this.codeExcerpt = node.codeExcerpt;
        this.parser = Parser.getInstance();
        this.parentParser = parser.parentParser;
        this.generator = new Generator.Empty(this);
        this.parent = new Empty();
        // this.parser.previousNode = this;
    }

    public enum NodeType
    {
        Empty,
        Unknown,
        Struct,
        IndexReference,
        VariableExpression,
        NumberExpression,
        BinaryExpression,
        VariableAssignment,
        VariableDeclaration,
        Prototype,
        Function,
        FunctionCall,
        BuiltinCall,
        StringExpression,
        Type,
        IfStatement,
        IfStatementConditional,
        ElseIfStatement,
        ElseIfStatementConditional,
        ElseStatement,
        ForLoop,
        WhileLoop,
        PhiVariable,
        ImportStatement,
        Return,
        ArrayExpression,
        NullExpression,
        ExternStatement,
        Reference,
        Dereference,
        ParenEncapsulation,
        CharExpression,
        Trait,
        Implement,
        Cast,
        ImplicitCast,
    }

    public virtual void addSpace(Util.Token space)
    {
        this.codeExcerpt += space.value;
    }

    public virtual void addParent(Node parent)
    {
        if (this.parent != null)
        {
            this.parent.removeChild(this);
        }
        if (parent != null)
        {
            parser.nodes.Remove(this);
            this.parent = parent;
        }
    }

    public virtual void addCode(Util.Token code)
    {
        this.codeExcerpt += code.value;
    }

    public virtual void addCode(string code)
    {
        this.codeExcerpt += code;
    }

    public virtual void addNL()
    {
        this.codeExcerpt += "\n";
    }

    public virtual void addChild(Node child)
    {
        // this.codeExcerpt += child.codeExcerpt;
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

    public virtual void checkExport()
    {
        //TODO: add public and private here
        if (parent.nodeType == AST.Node.NodeType.Empty && Parser.exportTypes.Contains(this.nodeType) && parser.parentParser != null)
        {
            DebugConsole.WriteAnsi("[yellow]adding export node to parent parser named " + parentParser.fileName + " [/]");
            this.parentParser.nodes.Add(this);
        }
        exportChecked = true;
    }
}
