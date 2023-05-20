namespace TranspilerGenerator;

public class StringExpression : Expression
{
    AST.StringExpression str;

    public StringExpression(AST.Node node) : base((AST.Expression)node)
    {
        this.str = (AST.StringExpression)node;
    }

    public override void generate()
    {
    }
}
