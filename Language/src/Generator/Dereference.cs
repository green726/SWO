namespace Generator;

using LLVMSharp;
using static IRGen;

public class Dereference : Expression
{
    public AST.Dereference deref;
    public Dereference(AST.Dereference expr) : base(expr)
    {
        this.deref = expr;
    }

    public override void generate()
    {
        checkPtrAndGen(valueStack.Pop());

        base.generate();
    }

    public void checkPtrAndGen(LLVMValueRef valInput)
    {
        LLVMTypeRef typeRef = LLVM.TypeOf(valInput);

        if (typeRef.TypeKind != LLVMTypeKind.LLVMPointerTypeKind)
        {
            valueStack.Push(valInput);
            return;
        }

        LLVMValueRef loadRef = LLVM.BuildLoad(builder, valInput, "loadtmp");
        checkPtrAndGen(valInput);
    }
}
