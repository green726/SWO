namespace Generator;

using LLVMSharp;
using static IRGen;

public class Return : Base
{
    AST.Return ret;

    public Return(AST.Node node)
    {
        this.ret = (AST.Return)node;
    }

    public override void generate()
    {
        ret.expr.generator.generate();
        LLVMValueRef retValue = valueStack.Pop();
        valueStack.Push(LLVM.BuildRet(builder, retValue));
    }
}
