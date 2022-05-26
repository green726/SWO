public class FunctionAST : ASTNode
{
    public PrototypeAST prototype;
    public List<ASTNode> body;


    public FunctionAST(PrototypeAST prototype, List<ASTNode>? body = null)
    {
        if (body == null) body = new List<ASTNode>();
        this.nodeType = NodeType.Function;
        this.prototype = prototype;
        this.body = body;

        Parser.nodes.Add(this);
    }

    public FunctionAST(PrototypeAST prototype, ASTNode body)
    {
        this.nodeType = NodeType.Function;
        this.prototype = prototype;
        this.body = new List<ASTNode>();
        this.body.Add(body);

        Parser.nodes.Add(this);
    }

    public override void removeChild(ASTNode child)
    {
        this.body.Remove(child);
    }

    public override void addChild(ASTNode child)
    {
        this.body.Add(child);
    }

}
