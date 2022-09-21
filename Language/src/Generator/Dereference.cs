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
        // LLVMValueRef valRef = valueStack.Pop();
        //
        // LLVMTypeRef typeRef = LLVM.TypeOf(valRef);
        //
        // LLVMTypeRef ptrType = LLVM.PointerType(typeRef, 0);
        //
        // LLVMValueRef ptrValue = LLVM.ConstPointerCast(valRef, ptrType);
        //
        // valueStack.Push(ptrValue);



        base.generate();
    }
}
