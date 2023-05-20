namespace TranspilerGenerator;

public abstract class Expression : Base
{
    public AST.Expression expr;

    protected Expression(AST.Expression expr)
    {
        this.expr = expr;
        // this.typeInfo = (StandardFormGeneratorTypeInformation)expr.type;
    }

    public override void generate()
    {
        base.generate();
    }

}

