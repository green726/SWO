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
        base.generate();
        DebugConsole.WriteAnsi("[blue]dereference genning[/]");
        this.deref.actualExpr.generator.generate();
        checkPtrAndGen(gen.valueStack.Pop());

    }

    public void checkPtrAndGen(LLVMValueRef valInput)
    {
        LLVMTypeRef typeRef = LLVM.TypeOf(valInput);

        if (typeRef.TypeKind != LLVMTypeKind.LLVMPointerTypeKind)
        {
            DebugConsole.WriteAnsi("[blue]pushing: [/]");
            DebugConsole.DumpValue(valInput);
            gen.valueStack.Push(valInput);
            return;
        }


        LLVMValueRef loadRef = LLVM.BuildLoad(gen.builder, valInput, "loadtmp");
        DebugConsole.WriteAnsi("[blue]load ref[/]");
        DebugConsole.DumpValue(loadRef);
        checkPtrAndGen(loadRef);
    }
}
