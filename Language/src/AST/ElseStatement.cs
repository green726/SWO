namespace AST;


public class ElseStatement : AST.Node
{
    // public Function elseFunc;
    // public FunctionCall elseCall;

    public AST.Node precedingStatement;
    public List<AST.Node> body;

    public ElseStatement(AST.Node parent, Util.Token token) : base(token)
    {
        this.nodeType = NodeType.ElseStatement;
        this.generator = new Generator.ElseStatement(this);

        this.parent = parent.parent;

        this.body = new List<AST.Node>();
        this.precedingStatement = new AST.Empty();
    }

    public override void addChild(AST.Node child)
    {
        body.Add(child);
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {
        if (child.value != "{" && child.value != "}")
        {
            throw ParserException.FactoryMethod($"Illegal child was added to an else statement", "Remove it", child, this);
        }
        base.addChild(child);
    }
}
