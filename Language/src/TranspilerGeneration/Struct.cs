namespace TranspilerGenerator;

public class Struct : Base
{
    AST.Struct str;
    public Struct(AST.Node node)
    {
        this.str = (AST.Struct)node;
    }

    public override void generate()
    {
        base.generate();
    }
}
