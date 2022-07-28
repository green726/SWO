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
                    varRef = namedValuesLLVM[varExpr.value]; if (varRef.Pointer != IntPtr.Zero)
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
        int arraySize = (int)LLVM.GetArrayLength(LLVM.GetElementType(varPtr.TypeOf()));
        Console.WriteLine("array size: " + arraySize);
        foreach (AST.Node node in varExpr.children)
        {
            AST.NumberExpression numExpr = (AST.NumberExpression)node;
            numExpr.value += Config.settings.variable.arrays.startIndex;
            if (Config.settings.variable.arrays.outOfBoundsErrorEnabled && numExpr.value > arraySize)
            {
                throw ParserException.FactoryMethod("Index out of range", "Make the index in range", varExpr);
            }
            numExpr.generator.generate();
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
        LLVMValueRef varRef = generateVarRef();
        return LLVM.BuildLoad(builder, varRef, varExpr.value);
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
