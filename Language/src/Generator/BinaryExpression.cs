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

        bool intMath = false;

        switch (binExpr.leftHand.nodeType)
        {
            case AST.Node.NodeType.BinaryExpression:
                leftHand = valueStack.Pop();
                break;
            case AST.Node.NodeType.NumberExpression:
                AST.NumberExpression numExpr = (AST.NumberExpression)binExpr.leftHand;
                binExpr.leftHand.generator.generate();
                leftHand = valueStack.Pop();
                if (numExpr.type.value == "int")
                {
                    intMath = true;
                }
                break;
            default:
                binExpr.leftHand.generator.generate();
                leftHand = valueStack.Pop();
                break;
        }

        switch (binExpr.rightHand.nodeType)
        {
            case AST.Node.NodeType.NumberExpression:
                AST.NumberExpression numExpr = (AST.NumberExpression)binExpr.rightHand;
                binExpr.rightHand.generator.generate();
                rightHand = valueStack.Pop();
                if (numExpr.type.value == "int")
                {
                    intMath = true;
                }
                break;
            default:
                binExpr.rightHand.generator.generate();
                rightHand = valueStack.Pop();
                break;
        }

        switch (binExpr.operatorType)
        {
            case AST.BinaryExpression.OperatorType.Add:
                if (intMath)
                {
                    ir = LLVM.BuildAdd(builder, leftHand, rightHand, "addtmp");
                }
                else
                {
                    ir = LLVM.BuildFAdd(builder, leftHand, rightHand, "addtmp");
                }
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
