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
            valRef = gen.valueStack.Pop();
            init = true;
        }

        DebugConsole.Write(this.varDec.type.value);
        this.varDec.type.generator.generate();
        typeLLVM = gen.typeStack.Pop();
        DebugConsole.WriteAnsi($"[red] type stack[/]");
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
                DebugConsole.Write("ZZZZZZZZZZZZZZ");
                DebugConsole.Write(allocaRef);
                gen.valueStack.Push(allocaRef);
                if (init)
                {
                    LLVMValueRef storeRef = LLVM.BuildStore(gen.builder, valRef, allocaRef);
                    gen.valueStack.Push(storeRef);
                }

                gen.addNamedValueInScope(varDec.name, allocaRef);
                // namedValuesLLVM.Add(varDec.name, allocaRef);
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
            }
        }

        DebugConsole.Write("adding var to named globals with name of" + varDec.name);
        // namedValuesAST.Add(varDec.name, varDec);
    }
}
