namespace TranspilerGenerator;

public class ImplicitCast : Expression
{
    private Expression exprGen;
    private TypeInformation desiredType;


    public ImplicitCast(AST.Expression expr, Expression exprGen, TypeInformation desiredType) : base(expr)
    {
        this.expr = expr;
        this.exprGen = exprGen;
        this.desiredType = desiredType;
    }

    public override void generate()
    {
    }
}
