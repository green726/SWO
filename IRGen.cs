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

    }

    public static void generatePrototype(Parser.PrototypeAST prototype)
    {

        //begin argument generation
        int argumentCount = prototype.arguments.Count();
        var arguments = new LLVMValueRef[argumentCount];

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



    }

    public static void generateFunction(Parser.FunctionAST func)
    {

    }

    public static void evaluateNode(Parser.ASTNode node)
    {
        switch (node.nodeType)
        {
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

            foreach (Parser.ASTNode child in node.children)
            {
                evaluateNode(child);
            }
            // LLVM.DumpValue(valueStack.Peek());
        }

        while (valueStack.Count > 0)
        {
            Console.WriteLine(valueStack.Pop().PrintValueToString());
        }

        LLVM.DumpModule(module);
    }

}
