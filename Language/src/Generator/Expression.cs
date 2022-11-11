namespace Generator;

using LLVMSharp;
using static IRGen;

public abstract class Expression : Base
{
    public AST.Expression expr;
    public GeneratorTypeInformation typeInfo;

    protected Expression(AST.Expression expr)
    {
        this.expr = expr;
        // this.typeInfo = (GeneratorTypeInformation)expr.type;
    }

    public override void generate()
    {
        DebugConsole.Write(expr.type);
        this.typeInfo = (GeneratorTypeInformation)expr.type;
        // if (expr.isReference || expr.isDereference)
        // {
        //     LLVMValueRef valRef = valueStack.Pop();
        //
        //     LLVMTypeRef typeRef = LLVM.TypeOf(valRef);
        //
        //     if (expr.isReference)
        //     {
        //         LLVMTypeRef ptrType = LLVM.PointerType(typeRef, 0);
        //
        //         LLVMValueRef ptrValue = LLVM.ConstPointerCast(valRef, ptrType);
        //
        //         valueStack.Push(ptrValue);
        //     }
        // }
        base.generate();
    }

}

