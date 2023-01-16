namespace Generator;

using LLVMSharp;
using static IRGen;

public class FunctionCall : Base
{
    AST.FunctionCall funcCall;

    AST.Prototype proto;

    public FunctionCall(AST.Node node)
    {
        this.funcCall = (AST.FunctionCall)node;
    }

    public override void generate()
    {
        base.generate();
        string nameToSearch = "";

        LLVMValueRef funcRef;
        if (funcCall.parser.declaredFuncs.ContainsKey(funcCall.functionName))
        {
            nameToSearch = funcCall.functionName;
            proto = gen.parser.declaredFuncs[nameToSearch];
            funcRef = LLVM.GetNamedFunction(gen.module, nameToSearch);
        }
        else
        {
            string altName = funcCall.functionName + funcCall.generateAltName();
            DebugConsole.WriteAnsi($"[red]alt name in function call gen:[/]");
            DebugConsole.Write(altName);
            nameToSearch = altName;

            // this.funcCall.type = (ParserTypeInformation)gen.parser.declaredFuncs[altName].returnType;
            (nameToSearch, proto, int traitFuncIdx) = gen.getDeclaredFunction(altName, funcCall);
            this.funcCall.type = (ParserTypeInformation)proto.returnType;
            DebugConsole.WriteAnsi("[red]result of irgen.getDeclaredFunction for funccall named: " + this.funcCall.functionName + " result: name: " + nameToSearch + " ret type: " + this.funcCall.type.value + "[/]");

            if (traitFuncIdx != 0)
            {
                DebugConsole.Write("value stack peek in trait func gen: " + gen.valueStack.Peek());
                funcRef = LLVM.BuildStructGEP(gen.builder, gen.valueStack.Peek(), (uint)traitFuncIdx, "traitFuncPtr");
            }
            else
            {
                funcRef = LLVM.GetNamedFunction(gen.module, nameToSearch);
            }
        }


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

        if (LLVM.CountParams(funcRef) != funcCall.args.Count && LLVM.IsFunctionVarArg(funcRef.TypeOf().GetElementType()) == false)
        {
            throw new GenException($"Incorrect # arguments passed ({funcCall.args.Count} passed but {LLVM.CountParams(funcRef)} required)", funcCall);
        }

        int argumentCount = funcCall.args.Count;
        var argsRef = new LLVMValueRef[argumentCount];
        DebugConsole.Write("genning args for func call named: " + funcCall.functionName);
        for (int i = 0; i < argumentCount; i++)
        {
            DebugConsole.Write("generating func call arg with value of: " + funcCall.args[i].value);

            if (funcCall.args[i].nodeType == AST.Node.NodeType.VariableExpression && !proto.variableArgument)
            {
                if (funcCall.args[i].type.isStruct || funcCall.args[i].type.isTrait)
                {
                    DebugConsole.Write("checking varExpr for casts");
                    AST.VariableExpression varExpr = (AST.VariableExpression)funcCall.args[i];
                    varExpr.desiredType = new GeneratorTypeInformation(proto.arguments.ElementAt(i).Value.value, gen.parser);
                    varExpr.checkForCasts();
                }
            }

            funcCall.args[i].generator.generate();
            argsRef[i] = gen.valueStack.Pop();
        }

        DebugConsole.WriteAnsi("[purple]funcCall stuff below[/]");
        DebugConsole.Write(LLVM.GetReturnType(funcRef.TypeOf()).GetReturnType().TypeKind);

        if (LLVM.GetReturnType(funcRef.TypeOf()).GetReturnType().TypeKind == LLVMTypeKind.LLVMVoidTypeKind)
        {
            DebugConsole.WriteAnsi("[purple]funcCallVoidRet[/]");
            gen.valueStack.Push(LLVM.BuildCall(gen.builder, funcRef, argsRef, ""));
        }
        else
        {
            DebugConsole.Write("Pushing func call named: " + funcCall.functionName);
            gen.valueStack.Push(LLVM.BuildCall(gen.builder, funcRef, argsRef, "calltmp"));
        }
    }

}
