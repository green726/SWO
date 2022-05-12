public class FunctionAST : ASTNode
{
    public PrototypeAST prototype;
    public List<ASTNode> body;


    public FunctionAST(PrototypeAST prototype, List<ASTNode>? body = default(List<ASTNode>))
    {
        this.nodeType = NodeType.Function;
        this.prototype = prototype;
        this.body = body;
    }

    public FunctionAST(PrototypeAST prototype, ASTNode body)
    {
        this.nodeType = NodeType.Function;
        this.prototype = prototype;
        this.body = new List<ASTNode>();
        this.body.Add(body);
    }

}
