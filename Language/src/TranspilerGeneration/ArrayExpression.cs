namespace TranspilerGenerator;

public class ArrayExpression : Expression
{
    public AST.ArrayExpression arrExpr;

    public ArrayExpression(AST.Expression node) : base(node)
    {
        this.arrExpr = (AST.ArrayExpression)node;
    }

    public override void generate()
    {
        base.generate();

    }
}
