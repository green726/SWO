namespace TranspilerGenerator;

public class NullExpression : Base
{
    public AST.NullExpression nullExpr;
    public NullExpression(AST.Node node)
    {
        this.nullExpr = (AST.NullExpression)node;
    }

    public override void generate()
    {
    }
}
