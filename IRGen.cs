using LLVMSharp;

public static class IRGen
{

    public static LLVMModuleRef module;

    private static LLVMBuilderRef builder;

    private static readonly Stack<LLVMValueRef> valueStack = new Stack<LLVMValueRef>();

    private static Dictionary<string, LLVMValueRef> namedValues = new Dictionary<string, LLVMValueRef>();

    public static void generateNumberExpression(NumberExpression numberExpression)
    {
        valueStack.Push(LLVM.ConstReal(LLVM.DoubleType(), numberExpression.value));
    }

    public static void generateBinaryExpression(BinaryExpression binaryExpression)
    {

        LLVMValueRef leftHand = new LLVMValueRef();
        LLVMValueRef rightHand = new LLVMValueRef();
        LLVMValueRef ir = new LLVMValueRef();

        switch (binaryExpression.leftHand.nodeType)
        {
            case ASTNode.NodeType.NumberExpression:
                NumberExpression leftHandExpr = (NumberExpression)binaryExpression.leftHand;
                leftHand = LLVM.ConstReal(LLVM.DoubleType(), leftHandExpr.value);
                break;
            case ASTNode.NodeType.BinaryExpression:
                leftHand = valueStack.Pop();
                break;
        }

        switch (binaryExpression.rightHand.nodeType)
        {
            case ASTNode.NodeType.NumberExpression:
                NumberExpression rightHandExpr = (NumberExpression)binaryExpression.rightHand;
                rightHand = LLVM.ConstReal(LLVM.DoubleType(), rightHandExpr.value);
                break;
        }

        switch (binaryExpression.operatorType)
        {
            case BinaryExpression.OperatorType.Add:
                ir = LLVM.BuildFAdd(builder, leftHand, rightHand, "addtmp");
                break;
        }

        valueStack.Push(ir);

        foreach (ASTNode child in binaryExpression.children)
        {
            Console.WriteLine(child.nodeType);
            evaluateNode(child);

        }

        LLVM.DumpValue(valueStack.Peek());
    }

    public static StringAST evaluatePrintFormat(FunctionCall printCall)
    {
        if (printCall.args[0].nodeType == ASTNode.NodeType.NumberExpression)
        {
            return new StringAST(new Util.Token(Util.TokenType.Keyword, "%f", 0, 0));
        }

        return new StringAST(new Util.Token(Util.TokenType.Keyword, "%f", 0, 0));
    }

    public static void generateBuiltinCall(FunctionCall builtIn)
    {

        StringAST printFormat;
        if (builtIn.functionName == "print")
        {
            //FIX: code errors in here somewhere

            builtIn.functionName = "printf";

            printFormat = evaluatePrintFormat(builtIn);
            Console.WriteLine("print format: " + printFormat);


            builtIn.addChildAtStart(printFormat);

        }

        LLVMValueRef funcRef = LLVM.GetNamedFunction(module, builtIn.functionName);

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

        for (int i = 1; i < argumentCount; i++)
        {
            evaluateNode(builtIn.args[i]);
            argsRef[i] = valueStack.Pop();
        }

        valueStack.Push(LLVM.BuildCall(builder, funcRef, argsRef, "calltmp"));

    }

    public static void generateFunctionCall(FunctionCall funcCall)
    {
        if (funcCall.builtIn)
        {
            generateBuiltinCall(funcCall);
            return;
        }
        LLVMValueRef funcRef = LLVM.GetNamedFunction(module, funcCall.functionName);

        if (funcRef.Pointer == IntPtr.Zero)
        {
            throw new Exception("Unknown function referenced");
        }

        if (LLVM.CountParams(funcRef) != funcCall.args.Count)
        {
            throw new Exception("Incorrect # arguments passed");
        }

        int argumentCount = funcCall.args.Count;
        var argsRef = new LLVMValueRef[argumentCount];
        for (int i = 0; i < argumentCount; ++i)
        {
            evaluateNode(funcCall.args[i]);
            argsRef[i] = valueStack.Pop();
        }

        valueStack.Push(LLVM.BuildCall(builder, funcRef, argsRef, "calltmp"));
    }


    public static void generatePrototype(PrototypeAST prototype)
    {
        //begin argument generation
        int argumentCount = prototype.arguments.Count();
        List<LLVMTypeRef> arguments = new List<LLVMTypeRef>();
        //check if function is already defined
        var function = LLVM.GetNamedFunction(module, prototype.name);

        if (function.Pointer != IntPtr.Zero)
        {
            // If func already has a body, reject this.
            if (LLVM.CountBasicBlocks(function) != 0)
            {
                throw new Exception($"redefinition of function named {prototype.name}");
            }

            // if func originally took a different number of args, reject.
            if (LLVM.CountParams(function) != argumentCount)
            {
                throw new Exception($"redefinition of function with different number of args (redfined to: {argumentCount})");
            }
        }

        else
        {

            foreach (KeyValuePair<Util.ClassType, string> arg in prototype.arguments)
            {
                switch (arg.Key)
                {
                    case Util.ClassType.Double:
                        arguments.Add(LLVM.DoubleType());
                        break;

                    case Util.ClassType.Int:
                        arguments.Add(LLVM.IntType(64));
                        break;
                        //TODO: implement strings as char arrays and chars as ints
                        // case Util.ClassType.String:
                        //     arguments.Add(LLVM.);
                        //     break;
                }

            }

            function = LLVM.AddFunction(module, prototype.name, LLVM.FunctionType(LLVM.DoubleType(), arguments.ToArray(), false));
            LLVM.SetLinkage(function, LLVMLinkage.LLVMExternalLinkage);

        }

        int argLoopIndex = 0;
        foreach (KeyValuePair<Util.ClassType, string> arg in prototype.arguments)
        {
            string argumentName = arg.Value;

            LLVMValueRef param = LLVM.GetParam(function, (uint)argLoopIndex);
            LLVM.SetValueName(param, argumentName);

            namedValues[argumentName] = param;
            argLoopIndex++;
        }

        valueStack.Push(function);
    }

    public static void generateFunction(FunctionAST funcNode)
    {
        //TODO: change this in the future once more variables are added
        namedValues.Clear();

        generatePrototype(funcNode.prototype);

        LLVMValueRef function = valueStack.Pop();

        LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(function, "body"));

        try
        {
            evaluateNode(funcNode.body.First());
        }
        catch (Exception)
        {
            LLVM.DeleteFunction(function);
            throw;
        }

        LLVM.BuildRet(builder, valueStack.Pop());

        LLVM.VerifyFunction(function, LLVMVerifierFailureAction.LLVMPrintMessageAction);

        valueStack.Push(function);

    }

    public static void evaluateNode(ASTNode node)
    {
        switch (node.nodeType)
        {
            case ASTNode.NodeType.Prototype:
                generatePrototype((PrototypeAST)node);
                break;
            case ASTNode.NodeType.Function:
                generateFunction((FunctionAST)node);
                break;
            case ASTNode.NodeType.BinaryExpression:
                generateBinaryExpression((BinaryExpression)node);
                break;
            case ASTNode.NodeType.FunctionCall:
                generateFunctionCall((FunctionCall)node);
                break;
            case ASTNode.NodeType.NumberExpression:
                generateNumberExpression((NumberExpression)node);
                break;
        }
    }

    public static void generateIR(List<ASTNode> nodes, LLVMBuilderRef _builder, LLVMModuleRef _module)
    {
        builder = _builder;
        module = _module;



        foreach (ASTNode node in nodes)
        {
            evaluateNode(node);


            // foreach (ASTNode child in node.children)
            // {
            //     evaluateNode(child);
            // }
            // Console.WriteLine("stack dump");
            // LLVM.DumpValue(valueStack.Peek());
        }

        Console.WriteLine("LLVM module dump below");
        LLVM.DumpModule(module);
        Console.WriteLine("");
    }

}
