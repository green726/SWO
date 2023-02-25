namespace Generator;

using LLVMSharp;

public class BinaryExpression : Base
{
    AST.BinaryExpression binExpr;

    public BinaryExpression(AST.BinaryExpression node)
    {
        this.binExpr = node;
    }

    public override void generate()
    {
        if (binExpr.rightHand.nodeType == AST.Node.NodeType.Empty)
        {
            //throw ParserException that says binary expression right hand is empty
            throw ParserException.FactoryMethod("Binary Expression right hand is empty", "Remove the binary expression | add a right hand", binExpr);
        }
        base.generate();
        // throw new Exception();
        DebugConsole.Write("rh type: " + binExpr.rightHand.nodeType);
        LLVMValueRef leftHand = new LLVMValueRef();
        LLVMValueRef rightHand = new LLVMValueRef();
        LLVMValueRef ir = new LLVMValueRef();

        bool intMath = false;

        switch (binExpr.leftHand.nodeType)
        {
            case AST.Node.NodeType.VariableExpression:
                binExpr.leftHand.generator.generate();
                leftHand = gen.recursiveDeReference(gen.valueStack.Pop());
                break;
            default:
                binExpr.leftHand.generator.generate();
                leftHand = gen.valueStack.Pop();
                break;
        }

        switch (binExpr.rightHand.nodeType)
        {
            // case AST.Node.NodeType.VariableExpression:
            //     binExpr.rightHand.generator.generate();
            //     rightHand = recursiveDeReference(valueStack.Pop());
            //     break;
            // case AST.Node.NodeType.BinaryExpression:
            //     binExpr.rightHand.generator.generate();
            //     rightHand = gen.valueStack.Pop();
            //     break;
            default:
                binExpr?.rightHand.generator.generate();
                rightHand = gen.valueStack.Pop();
                break;
        }

        if (LLVM.GetTypeKind(rightHand.TypeOf()) == LLVMTypeKind.LLVMIntegerTypeKind)
        {
            intMath = true;
        }

        if (intMath)
        {
            switch (binExpr.binOp.operatorType)
            {
                case BinaryOperator.OperatorType.Add:
                    ir = LLVM.BuildAdd(gen.builder, leftHand, rightHand, "addtmp");
                    break;
                case BinaryOperator.OperatorType.Subtract:
                    ir = LLVM.BuildSub(gen.builder, leftHand, rightHand, "subtmp");
                    break;
                case BinaryOperator.OperatorType.Equals:
                    ir = LLVM.BuildICmp(gen.builder, LLVMIntPredicate.LLVMIntEQ, leftHand, rightHand, "comparetmp");
                    break;
                case BinaryOperator.OperatorType.LessThan:
                    DebugConsole.DumpValue(leftHand);
                    LLVMValueRef lscmpRef = LLVM.BuildICmp(gen.builder, LLVMIntPredicate.LLVMIntSLT, leftHand, rightHand, "lessthantmp");
                    ir = lscmpRef;
                    // ir = LLVM.BuildUIToFP(builder, cmpRef, LLVMTypeRef.Int1Type(), "booltmp");
                    break;
                case BinaryOperator.OperatorType.GreaterThan:
                    DebugConsole.DumpValue(leftHand);
                    LLVMValueRef grcmpRef = LLVM.BuildICmp(gen.builder, LLVMIntPredicate.LLVMIntSGT, leftHand, rightHand, "lessthantmp");
                    ir = grcmpRef;
                    break;
                case BinaryOperator.OperatorType.Multiply:
                    ir = LLVM.BuildMul(gen.builder, leftHand, rightHand, "multmp");
                    break;
                case BinaryOperator.OperatorType.Divide:
                    ir = LLVM.BuildSDiv(gen.builder, leftHand, rightHand, "divtmp");
                    break;
            }
        }

        else
        {
            switch (binExpr.binOp.operatorType)
            {
                case BinaryOperator.OperatorType.Add:
                    ir = LLVM.BuildFAdd(gen.builder, leftHand, rightHand, "addtmp");
                    break;
                case BinaryOperator.OperatorType.Subtract:
                    ir = LLVM.BuildFSub(gen.builder, leftHand, rightHand, "subtmp");
                    break;
                case BinaryOperator.OperatorType.Equals:
                    ir = LLVM.BuildFCmp(gen.builder, LLVMRealPredicate.LLVMRealUEQ, leftHand, rightHand, "comparetmp");
                    break;
                case BinaryOperator.OperatorType.LessThan:
                    DebugConsole.DumpValue(leftHand);
                    LLVMValueRef cmpRef = LLVM.BuildFCmp(gen.builder, LLVMRealPredicate.LLVMRealULT, leftHand, rightHand, "lessthantmp");
                    ir = LLVM.BuildUIToFP(gen.builder, cmpRef, LLVMTypeRef.DoubleType(), "booltmp");
                    break;
                case BinaryOperator.OperatorType.Multiply:
                    ir = LLVM.BuildFMul(gen.builder, leftHand, rightHand, "multmp");
                    break;
                case BinaryOperator.OperatorType.Divide:
                    ir = LLVM.BuildFDiv(gen.builder, leftHand, rightHand, "divtmp");
                    break;
            }
        }

        DebugConsole.WriteAnsi("[red]binExpr dump below:[/] ");

        DebugConsole.DumpValue(ir);

        gen.valueStack.Push(ir);
    }
}
