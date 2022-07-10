namespace Generator;

public class Function : Base
{
    FunctionAST func;

    public Function(ASTNode node)
    {
        this.func = (FunctionAST)node;
    }

    public override void generate()
    {

    }
}
