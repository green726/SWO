namespace TranspilerGenerator;

public class Return : Base
{
    AST.Return ret;

    public Return(AST.Node node)
    {
        this.ret = (AST.Return)node;
    }

    public override void generate()
    {
    }
}
