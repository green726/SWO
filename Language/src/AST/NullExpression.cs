namespace AST;

public class NullExpression : Expression
{
    public NullExpression(Util.Token token) : base(token)
    {
        this.nodeType = AST.Node.NodeType.NullExpression;
        this.generator = new Generator.NullExpression(this);

        this.type = new AST.Type("void", this);
    }


    public NullExpression(Util.Token token, Node parent) : base(token, parent)
    {
        this.nodeType = AST.Node.NodeType.NullExpression;
        this.generator = new Generator.NullExpression(this);

        this.parent = parent;
        this.parent?.addChild(this);
        this.type = new AST.Type("void", this);
    }

}
