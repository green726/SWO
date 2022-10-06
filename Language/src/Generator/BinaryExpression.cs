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
                leftHand = gen.valueStack.Pop();
                break;
            case AST.Node.NodeType.VariableExpression:
                binExpr.leftHand.generator.generate();
                leftHand = gen.recursiveDeReference(gen.valueStack.Pop());
                break;
            default:
                binExpr.leftHand.generator.generate();
                leftHand = gen.valueStack.Pop();
                break;
        }

        switch (binExpr?.rightHand?.nodeType)
        {
            // case AST.Node.NodeType.VariableExpression:
            //     binExpr.rightHand.generator.generate();
            //     rightHand = recursiveDeReference(valueStack.Pop());
            //     break;
            default:
                binExpr?.rightHand?.generator.generate();
                rightHand = gen.valueStack.Pop();
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
                    ir = LLVM.BuildAdd(gen.builder, leftHand, rightHand, "addtmp");
                    break;
                case AST.BinaryExpression.OperatorType.Subtract:
                    ir = LLVM.BuildSub(gen.builder, leftHand, rightHand, "subtmp");
                    break;
                case AST.BinaryExpression.OperatorType.Equals:
                    ir = LLVM.BuildICmp(gen.builder, LLVMIntPredicate.LLVMIntEQ, leftHand, rightHand, "comparetmp");
                    break;
                case AST.BinaryExpression.OperatorType.LessThan:
                    DebugConsole.DumpValue(leftHand);
                    LLVMValueRef cmpRef = LLVM.BuildICmp(gen.builder, LLVMIntPredicate.LLVMIntSLT, leftHand, rightHand, "lessthantmp");
                    ir = cmpRef;
                    // ir = LLVM.BuildUIToFP(builder, cmpRef, LLVMTypeRef.Int1Type(), "booltmp");
                    break;
            }
        }

        else
        {
            switch (binExpr.operatorType)
            {
                case AST.BinaryExpression.OperatorType.Add:
                    ir = LLVM.BuildFAdd(gen.builder, leftHand, rightHand, "addtmp");
                    break;
                case AST.BinaryExpression.OperatorType.Subtract:
                    ir = LLVM.BuildFSub(gen.builder, leftHand, rightHand, "subtmp");
                    break;
                case AST.BinaryExpression.OperatorType.Equals:
                    ir = LLVM.BuildFCmp(gen.builder, LLVMRealPredicate.LLVMRealUEQ, leftHand, rightHand, "comparetmp");
                    break;
                case AST.BinaryExpression.OperatorType.LessThan:
                    DebugConsole.DumpValue(leftHand);
                    LLVMValueRef cmpRef = LLVM.BuildFCmp(gen.builder, LLVMRealPredicate.LLVMRealULT, leftHand, rightHand, "lessthantmp");
                    ir = LLVM.BuildUIToFP(gen.builder, cmpRef, LLVMTypeRef.DoubleType(), "booltmp");
                    break;
            }
        }

        DebugConsole.WriteAnsi("[red]binExpr dump below:[/] ");

        DebugConsole.DumpValue(ir);

        gen.valueStack.Push(ir);

        // foreach (ASTNode child in binaryExpression.children)
        // {
        //     evaluateNode(child);
        // }

        // DebugConsole.Write($"Value stack peek after bin below");
        // LLVM.DumpValue(valueStack.Peek());
        // DebugConsole.Write("");

    }
}
