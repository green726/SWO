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
        base.addChild(child);
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

        DebugConsole.WriteAnsi($"[green]adding child of type {child.nodeType} to for loop with parse iteration of {parseIteration}[/]");

        if (child.nodeType == NodeType.VariableExpression)
        {
            parseIteration++;
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
                break;
            case 1:
                if (child.nodeType != NodeType.BinaryExpression && child.nodeType != NodeType.VariableExpression)
                {
                    throw ParserException.FactoryMethod($"For Loop expected binary expression but found a {child.nodeType}", $"Remove the {child.nodeType} and replace it with a binary expression", child);
                }
                else if (child.nodeType == NodeType.BinaryExpression)
                {
                    this.loopCondition = (AST.BinaryExpression)child;
                }
                break;
            case 2:
                if (child.nodeType != NodeType.VariableAssignment)
                {
                    throw ParserException.FactoryMethod($"For Loop expected variable assignment but found a {child.nodeType}", $"Remove the {child.nodeType} and replace it with a variable assignment", child);
                }
                this.loopIteration = (AST.VariableAssignment)child;
                break;
        }
        parseIteration++;
    }

    public override void removeChild(AST.Node child)
    {
        DebugConsole.WriteAnsi($"[yellow]removing child of type {child.nodeType} from for loop with parse iteration (before decrement) of {parseIteration}[/]");
        base.removeChild(child);
        body.Remove(child);
        parseIteration -= 1;
    }
}
