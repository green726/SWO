namespace Generator;

using LLVMSharp;
using static IRGen;

public class VariableExpression : Expression
{
    AST.VariableExpression varExpr;

    public VariableExpression(AST.Node node) : base((AST.Expression)node)
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
                // varExpr.isReference = idxPar.isPointer;
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

    public string handleLLVMTypeBeingStupid(string type)
    {
        if (type.StartsWith("%") && type.Contains("="))
        {
            type = type.Remove(0, 1).Remove(type.IndexOf("=") - 1).Trim().TrimEnd('*');
        }
        else if (type.StartsWith("%"))
        {
            type = type.Remove(0, 1).Trim().TrimEnd('*');
        }
        return type;
    }

    public void updateCurrentStruct()
    {
        //BUG: below cant handle nested structs
        DebugConsole.Write("called update currentStruct");
        if (varExpr.children.Count() > 0)
        {
            LLVMValueRef strValRef = generateVarRef();
            DebugConsole.Write("typeof strvalref: " + strValRef.TypeOf());

            string strName = handleLLVMTypeBeingStupid(strValRef.TypeOf().PrintTypeToString());
            // string strName = strValRef.TypeOf().GetStructName();

            DebugConsole.WriteAnsi("[purple]strName: " + strName + "[/]");

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
        GeneratorTypeInformation exprGenTypeInfo = (GeneratorTypeInformation)varExpr.type;
        LLVMTypeRef varExprTypeLLVM = exprGenTypeInfo.genType();

        if (varExpr?.parent?.nodeType == AST.Node.NodeType.VariableExpression || varExpr?.parent?.nodeType == AST.Node.NodeType.IndexReference || varExpr?.parent?.parent?.nodeType == AST.Node.NodeType.VariableExpression && varExpr.parent.nodeType != AST.Node.NodeType.FunctionCall)
        {
            int num = 0;
            LLVMValueRef gepPtr = gen.valueStack.Pop();
            DebugConsole.Write("gepPtr: " + gepPtr);
            DebugConsole.Write("gepPtr type: " + gepPtr.TypeOf());

            num = gen.getStructFieldIndex(varExpr);
            DebugConsole.Write("got struct gep num: " + num);

            LLVMValueRef numGEPRef = LLVM.BuildStructGEP(gen.builder, gepPtr, (uint)num, "structGEPTmp");
            // LLVMValueRef[] indices = new LLVMValueRef[1] { LLVM.ConstInt(LLVM.Int32Type(), (ulong)num, false) };
            // LLVMValueRef numGEPRef = LLVM.BuildGEP(gen.builder, gepPtr, indices, "structGEPTmp");
            gen.valueStack.Push(numGEPRef);
            DebugConsole.Write("successfully built gep");

            checkIsPtr();

            if (!varExpr.isReference/*  && varExpr.children.Count() == 0 */)
            {
                LLVMValueRef numGEPRefLoad = LLVM.BuildLoad(gen.builder, numGEPRef, "structGEPLoad");
                gen.valueStack.Push(numGEPRefLoad);
                DebugConsole.Write(numGEPRefLoad);
                generateArrayGEP(numGEPRefLoad);
            }
            else
            {
                generateArrayGEP(numGEPRef);
            }


            //NOTE: incase this iteself is another struct set the current struct to this for the rest of its children
            updateCurrentStruct(gepPtr, num);

            //NOTE: gen this things children (incase it is a struct with more references) then return b/c we dont want to do other stuff below
            genChildren();
            return;
        }
        else
        {
            if (this.varExpr.isReference || this.varExpr.parent.nodeType == AST.Node.NodeType.Reference || this.varExpr.children.Count() > 0 || this.varExpr.type.isPointer || this.varExpr.type.isStruct || this.varExpr.type.isTrait)
            {
                DebugConsole.Write(this.varExpr.isReference);
                DebugConsole.Write("ptr var expr detected");
                LLVMValueRef varRef = generateVarRef();
                gen.valueStack.Push(varRef);
                generateArrayGEP(varRef);
            }
            else
            {
                if (!varExpr.isArrayRef)
                {
                    DebugConsole.Write("generating load ref");
                    LLVMValueRef loadRef = generateVarLoad();
                    gen.valueStack.Push(loadRef);
                }
                else
                {
                    generateArrayGEP(generateVarRef());
                }
            }

            updateCurrentStruct();

            genChildren();
        }
    }

    public void generateArrayGEP(LLVMValueRef ptr)
    {
        if (varExpr.isArrayRef)
        {
            DebugConsole.Write("gepping");
            DebugConsole.Write(ptr);
            //TODO: gep it
            LLVMValueRef arrayGepRef = LLVM.BuildStructGEP(gen.builder, ptr, (uint)varExpr.arrayIndex, "ArrayGEP");
            if (varExpr.parent.nodeType != AST.Node.NodeType.VariableAssignment)
            {
                LLVMValueRef loadRef = LLVM.BuildLoad(gen.builder, arrayGepRef, "ArrayGEPLoad");
                gen.valueStack.Push(loadRef);
            }
            // gen.valueStack.Push(arrayGepRef);
            DebugConsole.Write("done gepping");
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
