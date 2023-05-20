namespace TranspilerGenerator;

public class WhileLoop : Base
{
    public AST.WhileLoop loop;

    public WhileLoop(AST.WhileLoop loop) : base()
    {
        this.loop = loop;
    }

    public override void generate()
    {
    }
}
