namespace TranspilerGenerator;

public class Cast : Expression
{
    public AST.Cast cast;
    public Cast(AST.Expression expr) : base(expr)
    {
        this.cast = (AST.Cast)expr;
    }

    public override void generate()
    {
        base.generate();
    }
}
