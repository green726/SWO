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
                varExpr.isReference = varExprPar.isReference;
                break;
            case AST.Node.NodeType.IndexReference:
                AST.IndexReference idxPar = (AST.IndexReference)varExpr.parent;
                varExpr.isReference = idxPar.isPointer;
                break;
        }
    }

    public void updateCurrentStruct(LLVMValueRef parRef, int index)
    {
        if (varExpr.children.Count() > 0)
        {
            DebugConsole.Write("called update currentStruct with par: " + parRef);

            string strName = LLVM.GetAllocatedType(parRef).StructGetTypeAtIndex((uint)index).PrintTypeToString();
            DebugConsole.Write("str name: " + strName);

            if (strName.EndsWith("*"))
            {
                strName = strName.Remove(strName.Length - 1);
            }
            if (strName.StartsWith("%"))
            {
                strName = strName.Remove(0, 1);
            }

            int indexOfEquals = strName.IndexOf("=");
            DebugConsole.Write(indexOfEquals);
            strName = strName.Remove(indexOfEquals - 1);

            if (namedTypesAST.ContainsKey(strName))
            {
                AST.Struct strType = namedTypesAST[strName];
                currentStruct.Push(strType);
                DebugConsole.Write("updated currentStruct");
            }

            else
            {
                DebugConsole.Write("didnt update current struct");
            }
        }
    }

    public void updateCurrentStruct()
    {
        //BUG: below cant handle nested structs
        DebugConsole.Write("called update currentStruct");
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
                DebugConsole.Write("updated currentStruct");
            }

            else
            {
                DebugConsole.Write("didnt update current struct");
            }
        }
    }

    public override void generate()
    {
        DebugConsole.Write("genning varExpr with value of " + varExpr.value + " parent type of " + varExpr.parent?.nodeType + " and children count of " + varExpr.children.Count());
        if (varExpr?.parent?.nodeType == AST.Node.NodeType.VariableExpression || varExpr?.parent?.nodeType == AST.Node.NodeType.IndexReference)
        {
            int num = 0;
            LLVMValueRef gepPtr = valueStack.Pop();
            DebugConsole.Write("gepPtr: " + gepPtr);

            num = getStructFieldIndex(varExpr);
            DebugConsole.Write("got struct gep num: " + num);

            LLVMValueRef numGEPRef = LLVM.BuildStructGEP(builder, gepPtr, (uint)num, "structGEPTmp");
            valueStack.Push(numGEPRef);
            DebugConsole.Write(numGEPRef);

            checkIsPtr();

            if (!varExpr.isReference && varExpr.children.Count() == 0)
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
            if (this.varExpr.isReference || this.varExpr.children.Count() > 0)
            {
                DebugConsole.Write("ptr var expr detected");
                valueStack.Push(generateVarRef());
            }
            else
            {
                DebugConsole.Write("generating load ref");
                LLVMValueRef loadRef = generateVarLoad();
                DebugConsole.Write("genned load ref");
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
            DebugConsole.Write("genning varExpr child of type " + child.nodeType);
            child.generator.generate();
            DebugConsole.Write("genned varExpr child of type " + child.nodeType);
        }
    }


    public LLVMValueRef generateVarRef()
    {
        LLVMValueRef varRef = LLVM.GetNamedGlobal(module, varExpr.value);
        if (varRef.Pointer == IntPtr.Zero)
        {
            // DebugConsole.Write(String.Join(",", namedValuesLLVM.Keys.ToArray()));
            // DebugConsole.Write("named values LLVM count " + namedValuesLLVM.Count);
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
        if (namedValuesAST.ContainsKey(varExpr.value))
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
