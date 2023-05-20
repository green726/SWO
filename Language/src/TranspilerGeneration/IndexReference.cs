namespace TranspilerGenerator;

public class IndexReference : Base
{
    public AST.IndexReference idx;
    public IndexReference(AST.Node node)
    {
        this.idx = (AST.IndexReference)node;
    }

    public override void generate()
    {
        base.generate();
    }
}
