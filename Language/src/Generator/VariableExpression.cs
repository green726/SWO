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
                // case AST.Node.NodeType.Reference:
                //     varExpr.isReference = true;
                //     break;
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

            if (gen.namedTypesAST.ContainsKey(strName))
            {
                AST.Struct strType = gen.namedTypesAST[strName];
                gen.currentStruct.Push(strType);
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

            if (gen.namedTypesAST.ContainsKey(strName))
            {
                AST.Struct strType = gen.namedTypesAST[strName];
                gen.currentStruct.Push(strType);
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
        base.generate();
        DebugConsole.Write("genning varExpr with value of " + varExpr.value + " parent type of " + varExpr.parent?.nodeType + " and children count of " + varExpr.children.Count());
        if (varExpr?.parent?.nodeType == AST.Node.NodeType.VariableExpression || varExpr?.parent?.nodeType == AST.Node.NodeType.IndexReference || varExpr?.parent?.parent?.nodeType == AST.Node.NodeType.VariableExpression)
        {
            int num = 0;
            LLVMValueRef gepPtr = gen.valueStack.Pop();
            DebugConsole.Write("gepPtr: " + gepPtr);

            num = gen.getStructFieldIndex(varExpr);
            DebugConsole.Write("got struct gep num: " + num);


            LLVMValueRef numGEPRef = LLVM.BuildStructGEP(gen.builder, gepPtr, (uint)num, "structGEPTmp");
            gen.valueStack.Push(numGEPRef);
            DebugConsole.Write(numGEPRef);

            checkIsPtr();

            if (!varExpr.isReference/*  && varExpr.children.Count() == 0 */)
            {
                LLVMValueRef numGEPRefLoad = LLVM.BuildLoad(gen.builder, numGEPRef, "structGEPLoad");
                gen.valueStack.Push(numGEPRefLoad);
                DebugConsole.Write(numGEPRefLoad);
            }

            //NOTE: incase this iteself is another struct set the current struct to this for the rest of its children
            updateCurrentStruct(gepPtr, num);

            //NOTE: gen this things children (incase it is a struct with more references) then return b/c we dont want to do other stuff below
            genChildren();
            return;
        }
        else
        {
            if (this.varExpr.isReference || this.varExpr.parent.nodeType == AST.Node.NodeType.Reference || this.varExpr.children.Count() > 0 || this.varExpr.type.isPointer)
            {
                DebugConsole.Write("ptr var expr detected");
                gen.valueStack.Push(generateVarRef());
            }
            else
            {
                DebugConsole.Write("generating load ref");
                LLVMValueRef loadRef = generateVarLoad();
                DebugConsole.Write("genned load ref");
                gen.valueStack.Push(loadRef);
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
        LLVMValueRef varRef; /* = LLVM.GetNamedGlobal(module, varExpr.value); */
        // if (varRef.Pointer == IntPtr.Zero)
        // {
        // DebugConsole.Write(String.Join(",", namedValuesLLVM.Keys.ToArray()));
        // DebugConsole.Write("named values LLVM count " + namedValuesLLVM.Count);
        // if (namedMutablesLLVM.ContainsKey(varExpr.value))
        // {
        //     return namedMutablesLLVM[varExpr.value];
        // }
        // else
        // {
        // if (namedValuesLLVM.ContainsKey(varExpr.value))
        DebugConsole.Write(gen.fileName);
        if (gen.valueExistsInScope(varExpr.value))
        {
            DebugConsole.WriteAnsi("[green]detected variable in scope[/]");
            // varRef = namedValuesLLVM[varExpr.value];
            varRef = gen.getNamedValueInScope(varExpr.value);
            if (varRef.Pointer != IntPtr.Zero)
            {
                return varRef;
            }
        }
        else if (Config.settings.variable.declaration.reorder && Parser.getInstance().declaredGlobalsDict.ContainsKey(varExpr.value))
        {
            DebugConsole.WriteAnsi("[purple]stupid reordering[/]");
            LLVMBasicBlockRef currentBlock = LLVM.GetInsertBlock(gen.builder);
            AST.VariableDeclaration varDec = Parser.getInstance().declaredGlobalsDict[varExpr.value];
            varDec.generator.generate();
            varDec.generated = true;
            LLVM.PositionBuilderAtEnd(gen.builder, currentBlock);
            return generateVarRef();
        }
        // }

        throw GenException.FactoryMethod("An unknown variable was referenced", "Likely a typo", varExpr, true, varExpr.value);
        // }
        // else
        // {
        //     return varRef;
        // }

    }

    public LLVMValueRef generateVarLoad()
    {
        LLVMValueRef varRef = generateVarRef();
        if (varRef.TypeOf().TypeKind == LLVMTypeKind.LLVMPointerTypeKind)
        {
            return LLVM.BuildLoad(gen.builder, varRef, varExpr.value);
        }
        else
        {
            return varRef;
        }
    }
}
