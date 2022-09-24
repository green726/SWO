namespace AST;

public class ForLoop : AST.Node
{
    public PhiVariable index; // current index of loop
    public PhiVariable value; // current value of loop (if applicable)
    public List<AST.Node> body;
    public bool isBody = false;
    public int parseIteration;
    public bool complex = false;
    public bool valueLoop = false;
    public dynamic iterationObject; // object to be iterated over 
    public dynamic stepValue; // amount to iterate by 

    private Util.Token indexName;
    private Util.Token valueName;

    public ForLoop(Util.Token token, AST.Node parent) : base(token)
    {
        this.nodeType = NodeType.ForLoop;
        this.generator = new Generator.ForLoop(this);

        this.body = new List<AST.Node>();

        if (parent != null)
        {
            this.parent = parent;
            parent.addChild(this);
        }
        else
        {
            throw ParserException.FactoryMethod("An illegal parentless (top level) for loop was created", "Place the for loop within a function", this);
        }
        parseIteration = 0;
    }

    // for (int i = 0; i < 5; i++;) {
    //
    // }

    public void addComplex(Util.Token child)
    {
    }

    public void addSimple(Util.Token child)
    {
    }

    public void addComplex(AST.Node child)
    {
        if (parseIteration == 0)
        {
        }
    }

    public void addSimple(AST.Node child)
    {
    }

    public override void addChild(Util.Token child)
    {
        if (complex)
        {
            addComplex(child);
        }
        else
        {
            addSimple(child);
        }
        base.addChild(child);
    }

    public override void addChild(AST.Node child)
    {
        base.addChild(child);
    }

    public override void removeChild(AST.Node child)
    {
        base.removeChild(child);
        body.Remove(child);
    }
}

public class PhiVariable : AST.Node
{
    public string name;
    public Type type;
    public string value;
    public NumberExpression numExpr;

    public PhiVariable(AST.Node node) : base(node)
    {
        this.nodeType = NodeType.PhiVariable;
        this.generator = new Generator.PhiVariable(this);

        this.parent = node;
    }

    public void setValue(string value)
    {
        this.value = value;

        Util.Token numExprToken = new Util.Token(Util.TokenType.Int, value, this.line, this.column);
        this.numExpr = new NumberExpression(numExprToken, this);
    }

    public void setType(string type)
    {
        Util.Token typeToken = new Util.Token(Util.TokenType.Keyword, type, this.line, this.column);
        this.type = new Type(typeToken);
    }

    public void setName(string name)
    {
        DebugConsole.Write($"setting phi var name to {name}");
        this.name = name;
    }
}
