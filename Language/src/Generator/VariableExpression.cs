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

    public void checkIsPtr()
    {
        switch (varExpr.parent?.nodeType)
        {
            case AST.Node.NodeType.VariableExpression:
                AST.VariableExpression varExprPar = (AST.VariableExpression)varExpr.parent;
                varExpr.isPointer = varExprPar.isPointer;
                break;
            case AST.Node.NodeType.IndexReference:
                AST.IndexReference idxPar = (AST.IndexReference)varExpr.parent;
                varExpr.isPointer = idxPar.isPointer;
                break;
        }
    }

    public void updateCurrentStruct(LLVMValueRef parRef, int index)
    {
        if (varExpr.children.Count() > 0)
        {
            Console.WriteLine("called update currentStruct with par: " + parRef);

            string strName = LLVM.GetAllocatedType(parRef).StructGetTypeAtIndex((uint)index).PrintTypeToString();
            Console.WriteLine("str name: " + strName);

            if (strName.EndsWith("*"))
            {
                strName = strName.Remove(strName.Length - 1);
            }
            if (strName.StartsWith("%"))
            {
                strName = strName.Remove(0, 1);
            }

            int indexOfEquals = strName.IndexOf("=");
            Console.WriteLine(indexOfEquals);
            strName = strName.Remove(indexOfEquals - 1);

            if (namedTypesAST.ContainsKey(strName))
            {
                AST.Struct strType = namedTypesAST[strName];
                currentStruct.Push(strType);
                Console.WriteLine("updated currentStruct");
            }

            else
            {
                Console.WriteLine("didnt update current struct");
            }
        }
    }

    public void updateCurrentStruct()
    {
        //BUG: below cant handle nested structs
        Console.WriteLine("called update currentStruct");
        if (varExpr.children.Count() > 0)
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

            if (namedTypesAST.ContainsKey(strName))
            {
                AST.Struct strType = namedTypesAST[strName];
                currentStruct.Push(strType);
                Console.WriteLine("updated currentStruct");
            }

            else
            {
                Console.WriteLine("didnt update current struct");
            }
        }
    }

    public override void generate()
    {
        Console.WriteLine("genning varExpr with value of " + varExpr.value + " parent type of " + varExpr.parent?.nodeType + " and children count of " + varExpr.children.Count());
        if (varExpr?.parent?.nodeType == AST.Node.NodeType.VariableExpression || varExpr?.parent?.nodeType == AST.Node.NodeType.IndexReference)
        {
            int num = 0;
            LLVMValueRef gepPtr = valueStack.Pop();
            Console.WriteLine("gepPtr: " + gepPtr);

            num = getStructFieldIndex(varExpr);
            Console.WriteLine("got struct gep num: " + num);

            LLVMValueRef numGEPRef = LLVM.BuildStructGEP(builder, gepPtr, (uint)num, "structGEPTmp");
            valueStack.Push(numGEPRef);
            Console.WriteLine(numGEPRef);

            checkIsPtr();

            if (!varExpr.isPointer && varExpr.children.Count() == 0)
            {
                LLVMValueRef numGEPRefLoad = LLVM.BuildLoad(builder, numGEPRef, "structGEPLoad");
                valueStack.Push(numGEPRefLoad);
            }

            //NOTE: incase this iteself is another struct set the current struct to this for the rest of its children
            updateCurrentStruct(gepPtr, num);

            //NOTE: gen this things children (incase it is a struct with more references) then return b/c we dont want to do other stuff below
            genChildren();
            return;
        }
        else
        {
            if (this.varExpr.isPointer || this.varExpr.children.Count() > 0)
            {
                Console.WriteLine("ptr var expr detected");
                valueStack.Push(generateVarRef());
            }
            else
            {
                Console.WriteLine("generating load ref");
                LLVMValueRef loadRef = generateVarLoad();
                Console.WriteLine("genned load ref");
                valueStack.Push(loadRef);
            }

            updateCurrentStruct();

            genChildren();
        }
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
