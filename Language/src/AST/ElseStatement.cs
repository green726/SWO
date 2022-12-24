namespace AST;


public class ElseStatement : AST.Node
{
    // public Function elseFunc;
    // public FunctionCall elseCall;

    public List<AST.Node> body;
    // public AST.Node prevNode;

    public ElseStatement(Util.Token token, AST.Node parent) : base(token, parent)
    {
        this.nodeType = NodeType.ElseStatement;
        this.generator = new Generator.ElseStatement(this);

        this.parent = parent;
        this.body = new List<AST.Node>();

        // if (prevNode.nodeType != NodeType.IfStatement || prevNode.nodeType != NodeType.ElseIfStatement)
        // {
        //     throw ParserException.FactoryMethod($"Else statement must follow an if or else if statement (it follows a {prevNode.nodeType})", "Remove the nodes in between the if and else statements | Make sure the else is following an if", this, prevNode);
        // }
        // this.prevNode = prevNode;
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
