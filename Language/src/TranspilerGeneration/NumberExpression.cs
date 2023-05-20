namespace TranspilerGenerator;

public class NumberExpression : Base
{
    AST.NumberExpression numExpr;

    public NumberExpression(AST.Node node)
    {
        this.numExpr = (AST.NumberExpression)node;
    }

    public override void generate()
    {
        base.generate();
        gen.write(numExpr.value.ToString());
    }
}
