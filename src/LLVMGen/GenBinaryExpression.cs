using LLVMSharp;
using System;

public static class BinaryExpressionGen
{
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
                leftHand = IRGen.valueStack.Pop();
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
                ir = LLVM.BuildFAdd(IRGen.builder, leftHand, rightHand, "addtmp");
                break;
        }

        IRGen.valueStack.Push(ir);

        foreach (ASTNode child in binaryExpression.children)
        {
            Console.WriteLine(child.nodeType);
            IRGen.evaluateNode(child);

        }

        LLVM.DumpValue(IRGen.valueStack.Peek());
    }
}
