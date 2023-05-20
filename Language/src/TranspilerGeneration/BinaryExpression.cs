namespace TranspilerGenerator;

public class BinaryExpression : Base
{
    AST.BinaryExpression binExpr;

    public BinaryExpression(AST.BinaryExpression node)
    {
        this.binExpr = node;
    }

    public override void generate()
    {
        if (binExpr.rightHand.nodeType == AST.Node.NodeType.Empty)
        {
            //throw ParserException that says binary expression right hand is empty
            throw ParserException.FactoryMethod("Binary Expression right hand is empty", "Remove the binary expression | add a right hand", binExpr);
        }
        base.generate();

        this.binExpr.leftHand.generator.generate();
        gen.write(this.binExpr.binOp.ToString());
        this.binExpr.rightHand.generator.generate();
    }
}
