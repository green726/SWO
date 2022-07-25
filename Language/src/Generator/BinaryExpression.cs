namespace Generator;

using LLVMSharp;
using static IRGen;

public class BinaryExpression : Base
{
    AST.BinaryExpression binExpr;

    public BinaryExpression(AST.Node node)
    {
        this.binExpr = (AST.BinaryExpression)node;
    }

    public override void generate()
    {
        LLVMValueRef leftHand = new LLVMValueRef();
        LLVMValueRef rightHand = new LLVMValueRef();
        LLVMValueRef ir = new LLVMValueRef();

        switch (binExpr.leftHand.nodeType)
        {
            case AST.Node.NodeType.VariableExpression:
                AST.VariableExpression leftHandVarExpr = (AST.VariableExpression)binExpr.leftHand;
                binExpr.leftHand.generator.generate();
                leftHand = valueStack.Pop();
                break;
            case AST.Node.NodeType.NumberExpression:
                AST.NumberExpression leftHandExpr = (AST.NumberExpression)binExpr.leftHand;
                leftHand = LLVM.ConstReal(LLVM.DoubleType(), leftHandExpr.value);
                LLVM.DumpValue(leftHand);
                break;
            case AST.Node.NodeType.BinaryExpression:
                leftHand = valueStack.Pop();
                break;
            case AST.Node.NodeType.PhiVariable:
                binExpr.leftHand.generator.generate();
                leftHand = valueStack.Pop();
                break;
        }

        switch (binExpr.rightHand.nodeType)
        {
            case AST.Node.NodeType.VariableExpression:
                AST.VariableExpression rightHandVarExpr = (AST.VariableExpression)binExpr.rightHand;
                binExpr.rightHand.generator.generate();
                rightHand = valueStack.Pop();
                break;
            case AST.Node.NodeType.NumberExpression:
                AST.NumberExpression rightHandExpr = (AST.NumberExpression)binExpr.rightHand;
                rightHand = LLVM.ConstReal(LLVM.DoubleType(), rightHandExpr.value);
                break;
            case AST.Node.NodeType.PhiVariable:
                binExpr.rightHand.generator.generate();
                rightHand = valueStack.Pop();
                break;
        }

        switch (binExpr.operatorType)
        {
            case AST.BinaryExpression.OperatorType.Add:
                ir = LLVM.BuildFAdd(builder, leftHand, rightHand, "addtmp");
                break;
            case AST.BinaryExpression.OperatorType.Equals:
                ir = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealUEQ, leftHand, rightHand, "comparetmp");
                break;
            case AST.BinaryExpression.OperatorType.LessThan:
                LLVM.DumpValue(leftHand);
                LLVMValueRef cmpRef = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealULT, leftHand, rightHand, "comparetmp");
                ir = LLVM.BuildUIToFP(builder, cmpRef, LLVMTypeRef.DoubleType(), "booltmp");
                break;
        }

        valueStack.Push(ir);

        // foreach (ASTNode child in binaryExpression.children)
        // {
        //     evaluateNode(child);
        // }

        // Console.WriteLine($"Value stack peek after bin below");
        // LLVM.DumpValue(valueStack.Peek());
        // Console.WriteLine("");

    }
}
