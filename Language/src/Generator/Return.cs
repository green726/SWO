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
        base.generate();
        ret.expr.generator.generate();
        LLVMValueRef retValue = gen.valueStack.Pop();
        checkType(retValue.TypeOf());
        gen.valueStack.Push(LLVM.BuildRet(gen.builder, retValue));
    }

    private void checkType(LLVMTypeRef retType)
    {
        LLVMValueRef funcRef = LLVM.GetLastFunction(gen.module);

        LLVMTypeKind funcRetTypeKind = funcRef.TypeOf().GetReturnType().GetElementType().TypeKind;
        LLVMTypeKind retTypeKind = retType.TypeKind;

        if (funcRetTypeKind != retTypeKind)
        {
            throw GenException.FactoryMethod($"Return type({retType.ToString()}) does not match declared function type ({funcRef.TypeOf().GetReturnType().ToString()})", "Remove the return statement | Change the function return type", this.ret);
        }
    }
}
