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
        if (funcCall.builtIn)
        {
            generateBuiltinCall();
            return;
        }
        LLVMValueRef funcRef = LLVM.GetNamedFunction(module, funcCall.functionName);

        if (funcRef.Pointer == IntPtr.Zero)
        {
            throw new GenException($"Unknown function ({funcCall.functionName}) referenced", funcCall);
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

    public void generateBuiltinCall()
    {
        StringExpression printFormat;
        switch (funcCall.functionName)
        {
            case "print":
                funcCall.functionName = "printf";

                printFormat = evaluatePrintFormat(funcCall);
                // Console.WriteLine("successfully evaluated print format");

                funcCall.addChildAtStart(printFormat);
                // Console.WriteLine("appended child to start of print call");
                break;
            case "println":
                funcCall.functionName = "printf";

                printFormat = evaluatePrintFormat(funcCall);
                Console.WriteLine("successfully evaluated print format");

                funcCall.addChildAtStart(printFormat);

                AST.FunctionCall printNLCall = new AST.FunctionCall(new Util.Token(Util.TokenType.Keyword, "print!", funcCall.line, funcCall.column), new List<AST.Node>() { new AST.VariableExpression(new Util.Token(Util.TokenType.Keyword, "nl", funcCall.line, funcCall.column), parentRequired: false) }, true, funcCall.parent, false);
                break;
        }


        LLVMValueRef funcRef = LLVM.GetNamedFunction(module, funcCall.functionName);


        if (funcRef.Pointer == IntPtr.Zero)
        {
            throw new GenException($"Unknown function ({funcCall.functionName}) referenced", funcCall);
        }



        if (LLVM.CountParams(funcRef) != funcCall.args.Count)
        {
            throw new GenException($"Incorrect # arguments passed ({funcCall.args.Count} passed but {LLVM.CountParams(funcRef)} required)", funcCall);
        }

        int argumentCount = funcCall.args.Count;
        var argsRef = new LLVMValueRef[argumentCount];


        for (int i = 0; i < argumentCount; i++)
        {
            // Console.WriteLine("builtin with arg of: " + Parser.printASTRet(new List<ASTNode>() { builtIn.args[i] }));
            funcCall.args[i].generator.generate();
            argsRef[i] = valueStack.Pop();
            // Console.WriteLine(argsRef[i]);
            // Console.WriteLine($"evaluated builtin arg of {builtIn.args[i]}");
        }

        valueStack.Push(LLVM.BuildCall(builder, funcRef, argsRef, "calltmp"));
        // Console.WriteLine("successfully evaluated builtin call");

    }
}
