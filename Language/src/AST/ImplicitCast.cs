namespace AST;

public class ImplicitCast : Expression
{
    public Expression expr { get; set; }
    public TypeInformation desiredType { get; set; }
    public ImplicitCast(Expression expr, TypeInformation desiredType) : base(expr, true)
    {
        this.expr = expr;
        this.nodeType = NodeType.ImplicitCast;
        // this.generator = new Generator.ImplicitCast(this);
        this.desiredType = desiredType;
    }
}
