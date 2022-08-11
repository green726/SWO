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
        AST.VariableDeclaration originalVarDec = namedGlobalsAST[varAss.varExpr.value];

        if (originalVarDec.type.value == "string")
        {
            throw new GenException("mutable strings not yet supported", varAss);
        }

        // (LLVMValueRef valRef, LLVMTypeRef typeLLVM) = generateVariableValue();
        varAss.varExpr.isPointer = true;
        varAss.varExpr.generator.generate();
        LLVMValueRef targetValRef = valueStack.Pop();

        // LLVMValueRef loadRef = LLVM.BuildLoad(builder, namedMutablesLLVM[binVarName], binVarName);
        // valueStack.Push(loadRef);
        if (varAss.binReassignment)
        {
            this.varAss.bin.generator.generate();
            LLVMValueRef binValRef = valueStack.Pop();
            LLVMValueRef storeRef = LLVM.BuildStore(builder, binValRef, targetValRef);
            valueStack.Push(storeRef);
        }
        else
        {
            Console.WriteLine(varAss?.targetValue?.nodeType);
            varAss.targetValue.generator.generate();
            LLVMValueRef resultValRef = valueStack.Pop();
            LLVMValueRef storeRef = LLVM.BuildStore(builder, resultValRef, targetValRef);
            valueStack.Push(storeRef);
        }
    }
}
