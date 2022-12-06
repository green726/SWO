namespace AST;

public class ParenEncapsulation : Expression
{
    public Expression containedExpression { get; set; }

    //default constructor
    public ParenEncapsulation(Util.Token token, AST.Node parent) : base(token, parent)
    {
        this.containedExpression = new AST.Empty();
        this.nodeType = NodeType.ParenEncapsulation;
        this.parent = parent;
        this.parent.addChild(this);
        this.generator = new Generator.ParenEncapsulation(this);
    }

    public override void addChild(Node child)
    {
        if (containedExpression.nodeType == NodeType.Empty)
        {
            //throw error if child is not an expression
            if (!(child is Expression))
            {
                throw ParserException.FactoryMethod("Parenthese can only have an expression as a child", "Replace the inside of the parentheses with an expression", child, this);
            }
            DebugConsole.WriteAnsi("[blue]set paren encapsulation contained expression[/]");
            this.containedExpression = (Expression)child;
            base.addChild(child);
        }
        else
        {
            containedExpression.addChild(child);
        }
        // throw new Exception();
    }

    public override void addChild(Util.Token child)
    {
        //throw error if child is not )
        if (child.value != ")")
        {
            this.containedExpression.addChild(child);
        }
        else
        {
            base.addChild(child);
        }
    }

    public override void removeChild(Node child)
    {
        this.containedExpression = new AST.Empty();
        base.removeChild(child);
        // throw new Exception();
    }
}
