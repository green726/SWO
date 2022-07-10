namespace Generator;

public class BinaryExpression : Base
{
    global::BinaryExpression binExpr;

    public BinaryExpression(ASTNode node)
    {
        this.binExpr = (global::BinaryExpression)node;
    }

    public override void generate()
    {

    }
}
