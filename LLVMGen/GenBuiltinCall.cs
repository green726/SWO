using LLVMSharp;

public static class GenBuiltincall
{

    public static StringExpression evaluatePrintFormat(FunctionCall printCall)
    {
        if (printCall.args[0].nodeType == ASTNode.NodeType.NumberExpression)
        {
            return new StringExpression(new Util.Token(Util.TokenType.Keyword, "%f", 0, 0));
        }

        return new StringExpression(new Util.Token(Util.TokenType.Keyword, "%f", 0, 0));
    }

    public static void generateBuiltinCall(FunctionCall builtIn)
    {

        StringExpression printFormat;
        if (builtIn.functionName == "print")
        {
            builtIn.functionName = "printf";

            printFormat = evaluatePrintFormat(builtIn);


            builtIn.addChildAtStart(printFormat);

        }

        LLVMValueRef funcRef = LLVM.GetNamedFunction(IRGen.module, builtIn.functionName);

        if (funcRef.Pointer == IntPtr.Zero)
        {
            throw new Exception("Unknown function referenced");
        }

        if (LLVM.CountParams(funcRef) != builtIn.args.Count)
        {
            throw new Exception("Incorrect # arguments passed");
        }

        int argumentCount = builtIn.args.Count;
        var argsRef = new LLVMValueRef[argumentCount];

        for (int i = 0; i < argumentCount; i++)
        {
            IRGen.evaluateNode(builtIn.args[i]);
            argsRef[i] = IRGen.valueStack.Pop();
        }

        IRGen.valueStack.Push(LLVM.BuildCall(IRGen.builder, funcRef, argsRef, "calltmp"));

    }

}
