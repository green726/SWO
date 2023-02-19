namespace Generator;

using LLVMSharp;

public class ArrayExpression : Expression
{
    public AST.ArrayExpression arrExpr;

    public ArrayExpression(AST.Expression node) : base(node)
    {
        this.arrExpr = (AST.ArrayExpression)node;
    }

    public override void generate()
    {
        base.generate();

        arrExpr.type.size = arrExpr.length;

        LLVMTypeRef typeLLVM = typeInfo.genType();

        DebugConsole.Write("array expr type: " + typeLLVM);

        LLVMValueRef[] values = new LLVMValueRef[arrExpr.length];

        for (int i = 0; i < arrExpr.length; i++)
        {
            arrExpr.value[i].generator.generate();
            values[i] = gen.valueStack.Pop();
            DebugConsole.Write("arr expr value: " + values[i]);
        }

        gen.valueStack.Push(LLVM.ConstArray(typeLLVM, values));
    }
}
