using System.Collections.Generic;

public class FunctionAST : ASTNode
{
    public PrototypeAST prototype;
    public List<ASTNode> body;


    public FunctionAST(PrototypeAST prototype, List<ASTNode>? body = null, bool topLevel = true) : base(prototype)
    {
        if (body == null) body = new List<ASTNode>();
        this.nodeType = NodeType.Function;
        this.prototype = prototype;
        this.body = body;

        if (topLevel)
        {
            Parser.nodes.Add(this);
        }
    }

    public FunctionAST(PrototypeAST prototype, ASTNode body, bool topLevel = true) : base(prototype)

    {
        this.nodeType = NodeType.Function;
        this.prototype = prototype;
        this.body = new List<ASTNode>();
        this.body.Add(body);

        if (topLevel)
        {
            Parser.nodes.Add(this);
        }
    }

    public override void removeChild(ASTNode child)
    {
        base.removeChild(child);
        this.body.Remove(child);
    }

    public override void addChild(ASTNode child)
    {
        base.addChild(child);
        this.body.Add(child);
    }

}
