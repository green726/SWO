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
        AST.VariableDeclaration originalVarDec = namedGlobalsAST[varAss.name];

        if (originalVarDec.type.value == "string")
        {
            throw new GenException("mutable strings not yet supported", varAss);
        }

        // (LLVMValueRef valRef, LLVMTypeRef typeLLVM) = generateVariableValue();


        // LLVMValueRef loadRef = LLVM.BuildLoad(builder, namedMutablesLLVM[binVarName], binVarName);
        // valueStack.Push(loadRef);
        if (varAss.binReassignment)
        {
            this.varAss.bin.generator.generate();
            LLVMValueRef binValRef = valueStack.Pop();
            LLVMValueRef storeRef = LLVM.BuildStore(builder, binValRef, namedMutablesLLVM[varAss.name]);
            valueStack.Push(storeRef);
        }
        else
        {
            Console.WriteLine(varAss?.targetValue?.nodeType);
            varAss.targetValue.generator.generate();
            LLVMValueRef targetValRef = valueStack.Pop();
            LLVMValueRef storeRef = LLVM.BuildStore(builder, targetValRef, namedMutablesLLVM[varAss.name]);
            valueStack.Push(storeRef);
        }
    }


}
