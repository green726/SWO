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
        List<LLVMValueRef> childValueList = new List<LLVMValueRef>();
        foreach (AST.Node node in varExpr.children)
        {
            node.generator.generate();
            childValueList.Add(valueStack.Pop());
        }

        if (childValueList.Count() > 0)
        {
            LLVMValueRef[] arrIndices = { LLVM.ConstInt(LLVM.Int64Type(), (ulong)0, false) };

            LLVMValueRef varRef = LLVM.GetNamedGlobal(module, varExpr.value);
            if (varRef.Pointer == IntPtr.Zero)
            {
                // Console.WriteLine("var ref was pointer zero");
                if (namedMutablesLLVM.ContainsKey(varExpr.value))
                {
                    //code to load a stack mut
                    // valueStack.Push(LLVM.BuildLoad(builder, namedMutablesLLVM[varExpr.value], varExpr.value));
                    LLVMValueRef gepRef = LLVM.BuildInBoundsGEP(builder, namedMutablesLLVM[varExpr.value], arrIndices, this.varExpr.value);
                    valueStack.Push(gepRef);

                    return;
                }
                else
                {
                    if (namedValuesLLVM.ContainsKey(varExpr.value))
                    {
                        // varRef = namedValuesLLVM[varExpr.value];
                        if (varRef.Pointer != IntPtr.Zero)
                        {
                            LLVMValueRef gepRef = LLVM.BuildInBoundsGEP(builder, namedValuesLLVM[varExpr.value], arrIndices, this.varExpr.value);
                            valueStack.Push(gepRef);
                            return;
                        }
                    }
                    else if (Config.settings.variable.declaration.reorder && Parser.declaredGlobalsDict.ContainsKey(varExpr.value))
                    {
                        LLVMBasicBlockRef currentBlock = LLVM.GetInsertBlock(builder);
                        AST.VariableAssignment varAss = Parser.declaredGlobalsDict[varExpr.value];
                        varAss.generator.generate();
                        varAss.generated = true;
                        LLVM.PositionBuilderAtEnd(builder, currentBlock);
                        generate();
                        return;
                    }
                }

                throw GenException.FactoryMethod("An unknown variable was referenced", "Likely a typo", varExpr, true, varExpr.value);
            }
            else
            {
                LLVMValueRef gepRef = LLVM.BuildInBoundsGEP(builder, varRef, arrIndices, this.varExpr.value);
                valueStack.Push(gepRef);
                return;
            }

        }
        else
        {
            LLVMValueRef varRef = LLVM.GetNamedGlobal(module, varExpr.value);
            if (varRef.Pointer == IntPtr.Zero)
            {
                // Console.WriteLine("var ref was pointer zero");
                if (namedMutablesLLVM.ContainsKey(varExpr.value))
                {
                    //code to load a stack mut
                    valueStack.Push(LLVM.BuildLoad(builder, namedMutablesLLVM[varExpr.value], varExpr.value));
                    return;
                }
                else
                {
                    if (namedValuesLLVM.ContainsKey(varExpr.value))
                    {
                        varRef = namedValuesLLVM[varExpr.value];
                        if (varRef.Pointer != IntPtr.Zero)
                        {
                            valueStack.Push(varRef);
                            return;
                        }
                    }
                    else if (Config.settings.variable.declaration.reorder && Parser.declaredGlobalsDict.ContainsKey(varExpr.value))
                    {
                        LLVMBasicBlockRef currentBlock = LLVM.GetInsertBlock(builder);
                        AST.VariableAssignment varAss = Parser.declaredGlobalsDict[varExpr.value];
                        varAss.generator.generate();
                        varAss.generated = true;
                        LLVM.PositionBuilderAtEnd(builder, currentBlock);
                        generate();
                        return;
                    }
                }

                throw GenException.FactoryMethod("An unknown variable was referenced", "Likely a typo", varExpr, true, varExpr.value);
            }
            else
            {
                LLVMValueRef load = LLVM.BuildLoad(builder, varRef, varExpr.value);
                valueStack.Push(load);
                return;
            }

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
