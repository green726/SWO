namespace Generator;

using LLVMSharp;
using static IRGen;

public class VariableAssignment : Base
{
    AST.VariableAssignment varAss;

    public VariableAssignment(AST.Node node)
    {
        this.varAss = (AST.VariableAssignment)node;
    }

    public override void generate()
    {
        base.generate();
        // AST.VariableDeclaration originalVarDec = namedValuesAST[varAss.varExpr.value];
        //
        // if (originalVarDec.type.value == "string")
        // {
        //     throw new GenException("mutable strings not yet supported", varAss);
        // }

        // (LLVMValueRef valRef, LLVMTypeRef typeLLVM) = generateVariableValue();
        varAss.varExpr.isReference = true;
        varAss.varExpr.generator.generate();
        LLVMValueRef targetValRef = gen.valueStack.Pop();

        // LLVMValueRef loadRef = LLVM.BuildLoad(builder, namedMutablesLLVM[binVarName], binVarName);
        // valueStack.Push(loadRef);
        if (varAss.binReassignment)
        {
            DebugConsole.WriteAnsi("[green]bin reass detected[/]");
            this?.varAss?.bin?.generator.generate();
            LLVMValueRef binValRef = gen.valueStack.Pop();
            LLVMValueRef storeRef = LLVM.BuildStore(gen.builder, binValRef, targetValRef);
            gen.valueStack.Push(storeRef);
        }
        else
        {
            DebugConsole.WriteAnsi("[green]non bin reass detected[/]");
            varAss.targetValue.generator.generate();
            LLVMValueRef resultValRef = gen.valueStack.Pop();
            LLVMValueRef storeRef = LLVM.BuildStore(gen.builder, resultValRef, targetValRef);
            gen.valueStack.Push(storeRef);
        }
    }
}
