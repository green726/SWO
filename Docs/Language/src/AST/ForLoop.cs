namespace AST;

public class ForLoop : AST.Node
{
    public List<AST.Node> body;
    public bool isBody = false;

    //the variable in the loop
    public AST.VariableDeclaration varDec;
    //the condition for the loop (ie i < x;)
    public BinaryExpression loopCondition;
    //the iteration for the loop (ie i++;)
    public VariableAssignment loopIteration;
    public PhiVariable phiVarDec;
    private int parseIteration;


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

    public override void addChild(Util.Token child)
    {
        if (child.value == ";")
        {
            // parseIteration++;
        }
        else if (child.value == "(")
        {
        }
        else if (child.value == ")" || child.value == "{")
        {
            this.isBody = true;
        }
        else
        {
            throw ParserException.FactoryMethod($"Illegal child added to for loop", "Remove it", child, this);
        }
        base.addChild(child);
    }

    // for (int i = 0; i < 5; i++;) {
    //
    // }

    public override void addChild(AST.Node child)
    {
        base.addChild(child);

        if (isBody)
        {
            body.Add(child);
            return;
        }

        switch (parseIteration)
        {
            case 0:
                DebugConsole.WriteAnsi("[red]adding vardec to for loop[/]");
                if (child.nodeType != NodeType.VariableDeclaration)
                {
                    throw ParserException.FactoryMethod($"For Loop expected variable declaration but found a {child.nodeType}", $"Remove the {child.nodeType} and replace it with a variable declaration", child);
                }
                //TODO: enforce the type of the varDec to be a string
                this.varDec = (AST.VariableDeclaration)child;
                this.phiVarDec = new PhiVariable(varDec, this);
                break;
            case 1:
                if (child.nodeType != NodeType.VariableExpression)
                {
                    throw ParserException.FactoryMethod($"For Loop expected variable but found a {child.nodeType}", $"Remove the {child.nodeType} and replace it with a variable", child);
                }
                break;
            case 2:
                if (child.nodeType != NodeType.BinaryExpression)
                {
                    throw ParserException.FactoryMethod($"For Loop expected binary expression but found a {child.nodeType}", $"Remove the {child.nodeType} and replace it with a binary expression", child);
                }
                this.loopCondition = (AST.BinaryExpression)child;
                break;
            case 4:
                if (child.nodeType != NodeType.VariableAssignment)
                {
                    throw ParserException.FactoryMethod($"For Loop expected variable but found a {child.nodeType}", $"Remove the {child.nodeType} and replace it with a variable", child);
                }
                this.loopIteration = (AST.VariableAssignment)child;
                break;
        }
        parseIteration++;

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
    public AST.Expression value;
    public NumberExpression numExpr;

    public PhiVariable(AST.VariableDeclaration varDec, AST.Node parent) : base(varDec)
    {
        this.nodeType = NodeType.PhiVariable;
        this.generator = new Generator.PhiVariable(this);

        this.parent = parent;
    }

    // public void setValue(string value)
    // {
    //     this.value = value;
    //
    //     Util.Token numExprToken = new Util.Token(Util.TokenType.Int, value, this.line, this.column);
    //     this.numExpr = new NumberExpression(numExprToken, this);
    // }

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
