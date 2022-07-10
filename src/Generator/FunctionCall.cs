namespace Generator;

public class FunctionCall : Base
{
    global::FunctionCall funcCall;

    public FunctionCall(ASTNode node)
    {
        this.funcCall = (global::FunctionCall)node;
    }

    public override void generate()
    {

    }
}
