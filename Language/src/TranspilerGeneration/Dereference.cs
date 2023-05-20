namespace TranspilerGenerator;

public class Dereference : Expression
{
    public AST.Dereference deref;
    public Dereference(AST.Dereference expr) : base(expr)
    {
        this.deref = expr;
    }

    public override void generate()
    {
        base.generate();
    }
}
