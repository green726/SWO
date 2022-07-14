namespace Generator;

using LLVMSharp;
using static IRGen;

public class NumberExpression : Base
{
    AST.NumberExpression numExpr;

    public NumberExpression(AST.Node node)
    {
        this.numExpr = (AST.NumberExpression)node;
    }

    public override void generate()
    {
        switch (numExpr.type.value)
        {
            case "double":
                valueStack.Push(LLVM.ConstReal(LLVM.DoubleType(), numExpr.value));
                break;
            case "int":
                valueStack.Push(LLVM.ConstInt(LLVM.Int64Type(), (ulong)numExpr.value, true));
                break;
        }
    }
}
