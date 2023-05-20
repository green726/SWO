namespace TranspilerGenerator;


public class VariableExpression : Expression
{
    AST.VariableExpression varExpr;

    public VariableExpression(AST.Node node) : base((AST.Expression)node)
    {
        this.varExpr = (AST.VariableExpression)node;
    }

    public override void generate()
    {
        base.generate();
        gen.writeLine(varExpr.value);
    }
}
