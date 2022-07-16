namespace Generator;

using LLVMSharp;
using static IRGen;

public class VariableExpression : Base
{
    AST.VariableExpression varExpr;

    public VariableExpression(AST.Node node)
    {
        this.varExpr = (AST.VariableExpression)node;
    }

    public override void generate()
    {
        LLVMValueRef varRef = LLVM.GetNamedGlobal(module, varExpr.varName);
        if (varRef.Pointer == IntPtr.Zero)
        {
            Console.WriteLine("var ref was pointer zero");
            if (namedMutablesLLVM.ContainsKey(varExpr.varName))
            {
                //code to load a stack mut
                valueStack.Push(LLVM.BuildLoad(builder, namedMutablesLLVM[varExpr.varName], varExpr.varName));
                return;
            }
            else
            {
                if (namedValuesLLVM.ContainsKey(varExpr.varName)) {
                varRef = namedValuesLLVM[varExpr.varName];
                if (varRef.Pointer != IntPtr.Zero)
                {
                    valueStack.Push(varRef);
                    return;
                }
                }
                else if (Config.settings.variable.declaration.reorder && Parser.declaredGlobalsDict.ContainsKey(varExpr.varName))
                {
                    LLVMBasicBlockRef currentBlock = LLVM.GetInsertBlock(builder);
                    AST.VariableAssignment varAss = Parser.declaredGlobalsDict[varExpr.varName];
                    varAss.generator.generate();
                    varAss.generated = true;
                    LLVM.PositionBuilderAtEnd(builder, currentBlock);
                    generate();
                    return;
                }
            }

            throw GenException.FactoryMethod("An unknown variable was referenced", "Likely a typo", varExpr, true, varExpr.varName);
        }
        else
        {
            LLVMValueRef load = LLVM.BuildLoad(builder, varRef, varExpr.varName);
            valueStack.Push(load);
            return;
        }


        // if (namedGlobalsAST[varExpr.varName].type.value != "string")
        // {
        //     return;
        // }

        //NOTE: below stuff doesnt seem to do anything but maybe it will so leaving it be
        // LLVMValueRef[] arrIndices = { LLVM.ConstInt(LLVM.Int64Type(), (ulong)0, false) };
        // LLVMValueRef gepRef = LLVM.BuildInBoundsGEP(builder, globalRef, arrIndices, varExp.varName);
        // valueStack.Push(gepRef);

    }
}

public class PhiVariable : Base
{
    AST.PhiVariable phiVar;

    public PhiVariable(AST.Node node)
    {
        this.phiVar = (AST.PhiVariable)node;
    }

    public override void generate()
    {
        LLVMValueRef varRef = namedValuesLLVM[phiVar.name];
        if (varRef.Pointer != IntPtr.Zero)
        {
            valueStack.Push(varRef);
            return;
        }
        else
        {
            throw new GenException($"could not find local phi variable named {phiVar.name}", phiVar);
        }

    }

}
