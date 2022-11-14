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
        base.generate();
        reference.actualExpr.generator.generate();

        LLVMValueRef valRef = gen.valueStack.Pop();

        DebugConsole.WriteAnsi($"[blue]valueRef: [/]");
        DebugConsole.Write(valRef);

        LLVMTypeRef typeRef = LLVM.TypeOf(valRef);

        DebugConsole.WriteAnsi($"[blue]typeRef: [/]");
        DebugConsole.Write(typeRef);

        // if (typeRef.TypeKind == LLVMTypeKind.LLVMPointerTypeKind)
        // {
        //     base.generate();
        //     gen.valueStack.Push(valRef);
        //     return;
        // }

        LLVMTypeRef ptrType = LLVM.PointerType(typeRef, 0);

        LLVMValueRef ptrValue = LLVM.BuildPointerCast(gen.builder, valRef, ptrType, "ptrCast");

        DebugConsole.WriteAnsi("[red]ref pointer val: [/]");
        DebugConsole.Write(ptrValue);

        gen.valueStack.Push(ptrValue);
    }
}
