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

    public LLVMValueRef generateGEPNew(LLVMValueRef varPtr)
    {
        return new LLVMValueRef();
    }

    public void updateCurrentStruct()
    {
        Console.WriteLine("called update currentStruct");
        if (varExpr.children.Count() > 0 && !varExpr.isArrayIndexRef)
        {
            LLVMValueRef strValRef = generateVarRef();

            string strName = strValRef.TypeOf().PrintTypeToString();

            if (strName.EndsWith("*"))
            {
                strName = strName.Remove(strName.Length - 1);
            }
            if (strName.StartsWith("%"))
            {
                strName = strName.Remove(0, 1);
            }

            AST.Struct strType = namedTypesAST[strName];
            currentStruct.Push(strType);
        }
        Console.WriteLine("updated currentStruct");
    }

    public override void generate()
    {
        Console.WriteLine("genning varExpr with parent type of " + varExpr.parent?.nodeType);
        //NOTE: this will be hit if the parent is another var expr (this means that this varExpr is referencing a field of a struct (2nd or lower in foo.bar.cow))
        if (varExpr?.parent?.nodeType == AST.Node.NodeType.VariableExpression)
        {
            AST.VariableExpression varExprPar = (AST.VariableExpression)varExpr.parent;
            varExpr.isPointer = varExprPar.isPointer;
            int num = getStructFieldIndex(varExpr);
            Console.WriteLine("got struct gep num: " + num);

            LLVMValueRef strPtr = valueStack.Pop();
            Console.WriteLine(strPtr);

            LLVMValueRef numGEPRef = LLVM.BuildStructGEP(builder, strPtr, (uint)num, "structgeptmp");
            valueStack.Push(numGEPRef);

            if (!varExpr.isPointer)
            {
                LLVMValueRef numGEPRefLoad = LLVM.BuildLoad(builder, numGEPRef, "structgepload");
                valueStack.Push(numGEPRefLoad);
            }

            //NOTE: incase this iteself is another struct set the current struct to this for the rest of its children
            updateCurrentStruct();

            //NOTE: gen this things children (incase it is a struct with more references) then return b/c we dont want to do other stuff below
            genChildren();
            return;
        }

        //NOTE:below is incase this node is a top level struct reference (ie the first in foo.bar.cow)
        updateCurrentStruct();

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
        else if (this.varExpr.isPointer || this.varExpr.children.Count() > 0)
        {
            Console.WriteLine("ptr var expr detected");
            valueStack.Push(generateVarRef());

            genChildren();
            return;
        }
        Console.WriteLine("generating load ref");
        LLVMValueRef loadRef = generateVarLoad();
        Console.WriteLine("genned load ref");
        valueStack.Push(loadRef);

        genChildren();
    }

    public void genChildren()
    {
        foreach (AST.Node child in this.varExpr.children)
        {
            Console.WriteLine("genning varExpr child of type " + child.nodeType);
            child.generator.generate();
            Console.WriteLine("genned varExpr child of type " + child.nodeType);
        }
    }


    public LLVMValueRef generateVarRef()
    {
        LLVMValueRef varRef = LLVM.GetNamedGlobal(module, varExpr.value);
        if (varRef.Pointer == IntPtr.Zero)
        {
            // Console.WriteLine(String.Join(",", namedValuesLLVM.Keys.ToArray()));
            // Console.WriteLine("named values LLVM count " + namedValuesLLVM.Count);
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

        List<LLVMValueRef> childValueList = new List<LLVMValueRef>();

        LLVMTypeRef varType = varPtr.TypeOf();
        if (LLVM.GetTypeKind(varType) == LLVMTypeKind.LLVMPointerTypeKind)
        {
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



        return LLVM.BuildGEP(builder, varPtr, childValueList.ToArray(), varExpr.value);

        // throw ParserException.FactoryMethod("Too many index references were found in array reference", "Remove some index references (\"[1]\")", varExpr);
    }

    public LLVMValueRef generateVarLoad()
    {
        LLVMValueRef varRef = generateVarRef();
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
