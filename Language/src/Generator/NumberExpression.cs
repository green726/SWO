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
        base.generate();
        switch (numExpr.type.value)
        {
            case "double":
                gen.valueStack.Push(LLVM.ConstReal(LLVM.DoubleType(), numExpr.value));
                break;
            case "int":
                //TODO: figure out the context of the numExpr and set its int bits to align with that (assuming it fits, if not, throw an error)
                gen.valueStack.Push(LLVM.ConstInt(LLVM.Int32Type(), (ulong)numExpr.value, true));
                DebugConsole.WriteAnsi($"[red]numExpr {numExpr.value} and llvm val of {gen.valueStack.Peek()}[/]");
                break;
        }
    }
}
