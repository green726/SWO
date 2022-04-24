using LLVMSharp;

public static class IRGen
{

    private static LLVMModuleRef module;

    private static LLVMBuilderRef builder;

    private static readonly Stack<LLVMValueRef> valueStack = new Stack<LLVMValueRef>();

    private static Dictionary<string, LLVMValueRef> namedValues = new Dictionary<string, LLVMValueRef>();

    public static void generateNumberExpression(Parser.NumberExpression numberExpression)
    {

    }


    public static void generateBinaryExpression(Parser.BinaryExpression binaryExpression)
    {

        LLVMValueRef leftHand = new LLVMValueRef();
        LLVMValueRef rightHand = new LLVMValueRef();
        LLVMValueRef ir = new LLVMValueRef();

        switch (binaryExpression.leftHand.nodeType)
        {
            case Parser.ASTNode.NodeType.NumberExpression:
                Parser.NumberExpression leftHandExpr = (Parser.NumberExpression)binaryExpression.leftHand;
                leftHand = LLVM.ConstReal(LLVM.DoubleType(), leftHandExpr.value);
                break;
            case Parser.ASTNode.NodeType.BinaryExpression:
                leftHand = valueStack.Pop();
                break;
        }

        switch (binaryExpression.rightHand.nodeType)
        {
            case Parser.ASTNode.NodeType.NumberExpression:
                Parser.NumberExpression rightHandExpr = (Parser.NumberExpression)binaryExpression.rightHand;
                rightHand = LLVM.ConstReal(LLVM.DoubleType(), rightHandExpr.value);
                break;
        }



        switch (binaryExpression.operatorType)
        {
            case Parser.BinaryExpression.OperatorType.Add:
                ir = LLVM.BuildFAdd(builder, leftHand, rightHand, "addtmp");
                break;
        }

        valueStack.Push(ir);

        foreach (Parser.ASTNode child in binaryExpression.children)
        {
            Console.WriteLine(child.nodeType);
            evaluateNode(child);

        }

        LLVM.DumpValue(valueStack.Peek());
    }

    public static void generatePrototype(Parser.PrototypeAST prototype)
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

    public static void generateFunction(Parser.FunctionAST funcNode)
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

    public static void evaluateNode(Parser.ASTNode node)
    {
        switch (node.nodeType)
        {
            case Parser.ASTNode.NodeType.Function:
                generateFunction((Parser.FunctionAST)node);
                break;
            case Parser.ASTNode.NodeType.BinaryExpression:
                generateBinaryExpression((Parser.BinaryExpression)node);
                break;
            case Parser.ASTNode.NodeType.NumberExpression:
                break;
        }
    }

    public static void generateIR(List<Parser.ASTNode> nodes, LLVMBuilderRef _builder, LLVMModuleRef _module)
    {
        builder = _builder;
        module = _module;

        foreach (Parser.ASTNode node in nodes)
        {
            evaluateNode(node);

            // foreach (Parser.ASTNode child in node.children)
            // {
            //     evaluateNode(child);
            // }
            // LLVM.DumpValue(valueStack.Peek());
        }

        // while (valueStack.Count > 0)
        // {
        //     Console.WriteLine(valueStack.Pop().PrintValueToString());
        // }

        LLVM.DumpModule(module);
    }

}
