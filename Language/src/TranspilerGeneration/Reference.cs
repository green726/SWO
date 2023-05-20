namespace TranspilerGenerator;

public class Reference : Expression
{
    public AST.Reference reference;
    public Reference(AST.Reference expr) : base(expr)
    {
        this.reference = expr;
    }

    public override void generate()
    {
        base.generate();
    }
}
