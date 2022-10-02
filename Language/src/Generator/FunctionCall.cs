namespace Generator;


using LLVMSharp;
using static IRGen;

public class FunctionCall : Base
{
    AST.FunctionCall funcCall;

    public FunctionCall(AST.Node node)
    {
        this.funcCall = (AST.FunctionCall)node;
    }

    public override void generate()
    {
        string nameToSearch = "";
        if (Parser.declaredFuncs.ContainsKey(funcCall.functionName))
        {
            nameToSearch = funcCall.functionName;
        }
        else
        {
            string altName = funcCall.functionName + funcCall.generateAltName();

            this.funcCall.type = Parser.declaredFuncs[altName].returnType;

            DebugConsole.WriteAnsi($"[red]alt name in function call gen: {altName}[/]");
            nameToSearch = altName;
        }

        LLVMValueRef funcRef = LLVM.GetNamedFunction(module, nameToSearch);


        if (funcRef.Pointer == IntPtr.Zero)
        {
            // if (Config.settings.function.declaration.reorder && Parser.declaredFunctionDict.ContainsKey(funcCall.functionName) && LLVM.GetNamedFunction(module, funcCall.functionName).Pointer == IntPtr.Zero)
            // {
            //     LLVMBasicBlockRef currentBlock = LLVM.GetInsertBlock(builder);
            //     AST.Function calledFunc = Parser.declaredFunctionDict[funcCall.functionName];
            //     calledFunc.generator.generate();
            //     calledFunc.generated = true;
            //     LLVM.PositionBuilderAtEnd(builder, currentBlock);
            //     funcRef = LLVM.GetNamedFunction(module, funcCall.functionName);
            // }
            // else
            // {
            throw new GenException($"Unknown function ({funcCall.functionName}) referenced", funcCall);
            // }
        }

        if (LLVM.CountParams(funcRef) != funcCall.args.Count)
        {
            throw new GenException($"Incorrect # arguments passed ({funcCall.args.Count} passed but {LLVM.CountParams(funcRef)} required)", funcCall);
        }

        int argumentCount = funcCall.args.Count;
        var argsRef = new LLVMValueRef[argumentCount];
        for (int i = 0; i < argumentCount; ++i)
        {
            funcCall.args[i].generator.generate();
            argsRef[i] = valueStack.Pop();
        }

        valueStack.Push(LLVM.BuildCall(builder, funcRef, argsRef, "calltmp"));

    }

}
