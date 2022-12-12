namespace AST;


public class ElseStatement : AST.Node
{
    // public Function elseFunc;
    // public FunctionCall elseCall;

    public List<AST.Node> body;

    public ElseStatement(Util.Token token, AST.Node parent) : base(token, parent)
    {
        this.nodeType = NodeType.ElseStatement;
        this.generator = new Generator.ElseStatement(this);

        this.parent = parent;
        this.body = new List<AST.Node>();
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
