namespace Generator;
using LLVMSharp;
using static IRGen;

public class VariableDeclaration : Base
{
    public AST.VariableDeclaration varDec;
    public LLVMTypeRef typeLLVM;

    private bool init = false;

    public VariableDeclaration(AST.Node node)
    {
        this.varDec = (AST.VariableDeclaration)node;
    }

    public override void generate()
    {
        base.generate();
        DebugConsole.Write("Generating varDec with name of: " + varDec.name);
        if (varDec.generated) { return; }
        // if (varDec.type.value == "string")
        // {
        //     buildGlobalString();
        //     return;
        // }

        LLVMValueRef valRef = new LLVMValueRef();

        if (this.varDec.defaultValue.nodeType != AST.Node.NodeType.NullExpression)
        {
            this.varDec.defaultValue.generator.generate();
            DebugConsole.Write("generated vardec default val");
            valRef = gen.valueStack.Pop();
            init = true;
        }

        DebugConsole.Write(this.varDec.type.value);
        this.varDec.type.generator.generate();
        typeLLVM = gen.typeStack.Pop();

        if (varDec.type.isStruct && this.varDec.defaultValue.nodeType != AST.Node.NodeType.NullExpression)
        {
            // typeLLVM = gen.typeStack.Pop();
            DebugConsole.Write("creating struct val ref: " + valRef);
            valRef = LLVM.BuildLoad(gen.builder, valRef, "structAssignmentLoad");
            DebugConsole.Write("struct val ref: " + valRef);
        }
        else if (varDec.type.isStruct)
        {
            typeLLVM = gen.typeStack.Pop();
        }

        DebugConsole.WriteAnsi($"[red]var dec type stack[/]");
        DebugConsole.Write(typeLLVM);

        if (!varDec.mutable && typeLLVM.TypeKind != LLVMTypeKind.LLVMStructTypeKind && !varDec.local)
        {
            LLVMValueRef constRef = LLVM.AddGlobal(gen.module, typeLLVM, varDec.name);
            if (init)
            {
                LLVM.SetInitializer(constRef, valRef);
            }
            gen.valueStack.Push(constRef);
        }
        else
        {
            if (varDec.local)
            {
                LLVMValueRef allocaRef = LLVM.BuildAlloca(gen.builder, typeLLVM, varDec.name);
                DebugConsole.Write("allocaRef: " + allocaRef);
                gen.valueStack.Push(allocaRef);
                if (init)
                {
                    LLVMValueRef storeRef = LLVM.BuildStore(gen.builder, valRef, allocaRef);
                    gen.valueStack.Push(storeRef);
                }

                gen.addNamedValueInScope(varDec.name, allocaRef);
                // namedValuesLLVM.Add(varDec.name, allocaRef);

                if (varDec.type.isStruct)
                {
                    Base strGen = gen.parser.declaredStructs[varDec.type.value].generator;
                    ((Struct)strGen).createImplValues(allocaRef, varDec.name);
                }
            }
            else
            {
                if (!gen.mainBuilt)
                {
                    DebugConsole.Write("adding to main nodes to build");
                    gen.nodesToBuild.Add(varDec);
                    return;
                }
                LLVM.PositionBuilderAtEnd(gen.builder, gen.mainEntryBlock);
                DebugConsole.Write($"building for mutable var with name of {varDec.name} and type of");
                DebugConsole.DumpType(typeLLVM);
                DebugConsole.Write();
                LLVMValueRef allocaRef = LLVM.BuildAlloca(gen.builder, typeLLVM, varDec.name);
                gen.valueStack.Push(allocaRef);
                DebugConsole.Write("built and pushed alloca: " + allocaRef);
                if (init)
                {
                    DebugConsole.Write("store ref target: " + valRef);
                    LLVMValueRef storeRef = LLVM.BuildStore(gen.builder, valRef, allocaRef);
                    gen.valueStack.Push(storeRef);
                    DebugConsole.Write("built and pushed store ref: " + storeRef);
                }

                gen.addNamedValueInScope(varDec.name, allocaRef);
                // namedValuesLLVM.Add(varDec.name, allocaRef);

                if (varDec.type.isStruct)
                {
                    Base strGen = gen.parser.declaredStructs[varDec.type.value].generator;
                    ((Struct)strGen).createImplValues(allocaRef, varDec.name);
                }
            }
        }



        DebugConsole.Write("adding var to named globals with name of" + varDec.name);
        // namedValuesAST.Add(varDec.name, varDec);
    }
}
