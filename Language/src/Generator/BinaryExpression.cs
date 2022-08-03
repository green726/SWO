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
            default:
                binExpr.leftHand.generator.generate();
                leftHand = valueStack.Pop();
                break;
        }

        switch (binExpr.rightHand.nodeType)
        {
            default:
                binExpr.rightHand.generator.generate();
                rightHand = valueStack.Pop();
                break;
        }

        if (LLVM.GetTypeKind(rightHand.TypeOf()) == LLVMTypeKind.LLVMIntegerTypeKind)
        {
            intMath = true;
        }

        if (intMath)
        {
            switch (binExpr.operatorType)
            {
                case AST.BinaryExpression.OperatorType.Add:
                    ir = LLVM.BuildAdd(builder, leftHand, rightHand, "addtmp");
                    break;
                case AST.BinaryExpression.OperatorType.Subtract:
                    ir = LLVM.BuildSub(builder, leftHand, rightHand, "subtmp");
                    break;
                case AST.BinaryExpression.OperatorType.Equals:
                    ir = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, leftHand, rightHand, "comparetmp");
                    break;
                case AST.BinaryExpression.OperatorType.LessThan:
                    LLVM.DumpValue(leftHand);
                    LLVMValueRef cmpRef = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealULT, leftHand, rightHand, "comparetmp");
                    ir = LLVM.BuildUIToFP(builder, cmpRef, LLVMTypeRef.DoubleType(), "booltmp");
                    break;
            }
        }

        else
        {

            switch (binExpr.operatorType)
            {
                case AST.BinaryExpression.OperatorType.Add:
                    ir = LLVM.BuildFAdd(builder, leftHand, rightHand, "addtmp");
                    break;
                case AST.BinaryExpression.OperatorType.Subtract:
                    ir = LLVM.BuildFSub(builder, leftHand, rightHand, "subtmp");
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
        }

        Spectre.Console.AnsiConsole.MarkupLine("[red]binExpr dump below:[/] ");

        LLVM.DumpValue(ir);

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
