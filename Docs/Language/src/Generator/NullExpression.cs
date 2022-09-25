namespace Generator;

using LLVMSharp;
using static IRGen;

public class NullExpression : Base
{
    public AST.NullExpression nullExpr;
    public NullExpression(AST.Node node)
    {
        this.nullExpr = (AST.NullExpression)node;
    }

    public override void generate()
    {
        valueStack.Push(LLVM.ConstNull(LLVMTypeRef.VoidType()));
    }
}
