namespace Generator;

using LLVMSharp;
using static IRGen;

public class Reference : Expression
{
    public AST.Reference reference;
    public Reference(AST.Reference expr) : base(expr)
    {
        this.reference = expr;
    }

    public override void generate()
    {
        reference.actualExpr.generator.generate();

        LLVMValueRef valRef = valueStack.Pop();

        DebugConsole.WriteAnsi($"[blue]valueRef: {valRef}[/]");

        LLVMTypeRef typeRef = LLVM.TypeOf(valRef);

        DebugConsole.WriteAnsi($"[blue]typeRef: {typeRef} type kind: {typeRef.TypeKind}[/]");

        if (typeRef.TypeKind == LLVMTypeKind.LLVMPointerTypeKind)
        {
            base.generate();
            valueStack.Push(valRef);
            return;
        }

        LLVMTypeRef ptrType = LLVM.PointerType(typeRef, 0);

        LLVMValueRef ptrValue = LLVM.ConstPointerCast(valRef, ptrType);

        DebugConsole.WriteAnsi("[red]ref pointer val: [/]");
        DebugConsole.Write(ptrValue);

        valueStack.Push(ptrValue);

        base.generate();
    }
}
