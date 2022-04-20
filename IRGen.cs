using LLVMSharp;

public static class IRGen
{

    private static LLVMModuleRef module;

    private static LLVMBuilderRef builder;

    private static readonly Stack<LLVMValueRef> valueStack = new Stack<LLVMValueRef>();

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
        }

        while (valueStack.Count > 0)
        {
            Console.WriteLine(valueStack.Pop().PrintValueToString());
        }

        LLVM.DumpModule(module);
    }

}
