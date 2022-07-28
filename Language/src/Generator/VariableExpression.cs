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
        if (this.varExpr.isArrayIndexRef)
        {
            LLVMValueRef varRef = generateVarRef();
            LLVMValueRef gepRef = generateGEP(varRef);
            LLVMValueRef gepLoadRef = LLVM.BuildLoad(builder, gepRef, varExpr.value);
            valueStack.Push(gepLoadRef);
            return;
        }
        LLVMValueRef loadRef = generateVarLoad();
        valueStack.Push(loadRef);

        // if (namedGlobalsAST[varExpr.varName].type.value != "string")
        // {
        //     return;
        // }

        //NOTE: below stuff doesnt seem to do anything but maybe it will so leaving it be
        // LLVMValueRef[] arrIndices = { LLVM.ConstInt(LLVM.Int64Type(), (ulong)0, false) };
        // LLVMValueRef gepRef = LLVM.BuildGEP(builder, globalRef, arrIndices, varExp.varName);
        // valueStack.Push(gepRef);

    }

    public LLVMValueRef generateVarRef()
    {
        LLVMValueRef varRef = LLVM.GetNamedGlobal(module, varExpr.value);
        if (varRef.Pointer == IntPtr.Zero)
        {
            if (namedMutablesLLVM.ContainsKey(varExpr.value))
            {
                return namedMutablesLLVM[varExpr.value];
            }
            else
            {
                if (namedValuesLLVM.ContainsKey(varExpr.value))
                {
                    varRef = namedValuesLLVM[varExpr.value];
                    if (varRef.Pointer != IntPtr.Zero)
                    {
                        return varRef;
                    }
                }
                else if (Config.settings.variable.declaration.reorder && Parser.declaredGlobalsDict.ContainsKey(varExpr.value))
                {
                    LLVMBasicBlockRef currentBlock = LLVM.GetInsertBlock(builder);
                    AST.VariableAssignment varAss = Parser.declaredGlobalsDict[varExpr.value];
                    varAss.generator.generate();
                    varAss.generated = true;
                    LLVM.PositionBuilderAtEnd(builder, currentBlock);
                    return generateVarRef();
                }
            }

            throw GenException.FactoryMethod("An unknown variable was referenced", "Likely a typo", varExpr, true, varExpr.value);
        }
        else
        {
            return varRef;
        }

    }

    public LLVMValueRef generateGEP(LLVMValueRef varPtr)
    {
        // LLVMValueRef valueRef =
        List<LLVMValueRef> childValueList = new List<LLVMValueRef>();
        childValueList.Add(LLVM.ConstInt(LLVMTypeRef.Int64Type(), 0, false));
        foreach (AST.Node node in varExpr.children)
        {
            Console.WriteLine(node.nodeType);
            node.generator.generate();
            childValueList.Add(valueStack.Pop());
        }


        if (childValueList.Count() <= 1)
        {
            throw ParserException.FactoryMethod("No index references were found in array reference", "Add in index reference (\"[1]\")", varExpr);
        }
        return LLVM.BuildInBoundsGEP(builder, varPtr, childValueList.ToArray(), varExpr.value);

        // throw ParserException.FactoryMethod("Too many index references were found in array reference", "Remove some index references (\"[1]\")", varExpr);
    }

    public LLVMValueRef generateVarLoad()
    {
        LLVMValueRef varRef = LLVM.GetNamedGlobal(module, varExpr.value);
        if (varRef.Pointer == IntPtr.Zero)
        {
            // Console.WriteLine("var ref was pointer zero");
            if (namedMutablesLLVM.ContainsKey(varExpr.value))
            {
                //code to load a stack mut
                return LLVM.BuildLoad(builder, namedMutablesLLVM[varExpr.value], varExpr.value);
            }
            else
            {
                if (namedValuesLLVM.ContainsKey(varExpr.value))
                {
                    varRef = namedValuesLLVM[varExpr.value];
                    if (varRef.Pointer != IntPtr.Zero)
                    {
                        return varRef;
                    }
                }
                else if (Config.settings.variable.declaration.reorder && Parser.declaredGlobalsDict.ContainsKey(varExpr.value))
                {
                    LLVMBasicBlockRef currentBlock = LLVM.GetInsertBlock(builder);
                    AST.VariableAssignment varAss = Parser.declaredGlobalsDict[varExpr.value];
                    varAss.generator.generate();
                    varAss.generated = true;
                    LLVM.PositionBuilderAtEnd(builder, currentBlock);
                    return generateVarLoad();
                }
            }

            throw GenException.FactoryMethod("An unknown variable was referenced", "Likely a typo", varExpr, true, varExpr.value);
        }
        else
        {
            LLVMValueRef load = LLVM.BuildLoad(builder, varRef, varExpr.value);
            return load;
        }

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
