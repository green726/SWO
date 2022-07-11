namespace Generator;

public class Function : Base
{
    AST.Function func;

    public Function(AST.Node node)
    {
        this.func = (AST.Function)node;
    }

    public override void generate()
    {

    }
}
