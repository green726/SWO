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
            Console.WriteLine("genned GEP");
            LLVMValueRef gepLoadRef = LLVM.BuildLoad(builder, gepRef, varExpr.value);
            valueStack.Push(gepLoadRef);
            Console.WriteLine(gepLoadRef);
            return;
        }
        else if (this.varExpr.isPointer)
        {
            valueStack.Push(generateVarRef());
            return;
        }
        Console.WriteLine("generating load ref");
        LLVMValueRef loadRef = generateVarLoad();
        Console.WriteLine("genned load ref");
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
                    varRef = namedValuesLLVM[varExpr.value];
                    if (varRef.Pointer != IntPtr.Zero)
                    {
                        Console.WriteLine("hello 1");
                        return varRef;
                    }
                }
                else if (Config.settings.variable.declaration.reorder && Parser.declaredGlobalsDict.ContainsKey(varExpr.value))
                {
                    LLVMBasicBlockRef currentBlock = LLVM.GetInsertBlock(builder);
                    AST.VariableDeclaration varDec = Parser.declaredGlobalsDict[varExpr.value];
                    varDec.generator.generate();
                    varDec.generated = true;
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
        Console.WriteLine("varPtr: " + varPtr);

        List<LLVMValueRef> childValueList = new List<LLVMValueRef>();

        LLVMTypeRef varType = varPtr.TypeOf();
        if (LLVM.GetTypeKind(varType) == LLVMTypeKind.LLVMPointerTypeKind)
        {
            Console.WriteLine("pointer detected");
            Console.WriteLine(varPtr);
            foreach (AST.Node node in varExpr.children)
            {
                AST.NumberExpression numExpr = (AST.NumberExpression)node;
                numExpr.value += Config.settings.variable.arrays.startIndex;
                numExpr.generator.generate();
                childValueList.Add(valueStack.Pop());
            }
            if (childValueList.Count() < 1)
            {
                throw ParserException.FactoryMethod("No index references were found in array reference", "Add in index reference (\"[1]\")", varExpr);
            }
        }
        else
        {
            childValueList.Add(LLVM.ConstInt(LLVMTypeRef.Int64Type(), 0, false));
            int arraySize = (int)LLVM.GetArrayLength(varType);
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
        }



        Console.WriteLine("building GEP with var ptr of: " + varPtr);
        return LLVM.BuildGEP(builder, varPtr, childValueList.ToArray(), varExpr.value);

        // throw ParserException.FactoryMethod("Too many index references were found in array reference", "Remove some index references (\"[1]\")", varExpr);
    }

    public LLVMValueRef generateVarLoad()
    {
        LLVMValueRef varRef = generateVarRef();
        Console.WriteLine(varRef);
        if (namedGlobalsAST.ContainsKey(varExpr.value))
        {
            return LLVM.BuildLoad(builder, varRef, varExpr.value);
        }
        else
        {
            return varRef;
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
