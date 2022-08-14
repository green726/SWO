namespace Generator;

using LLVMSharp;
using static IRGen;

public class ArrayExpression : Base
{
    public AST.ArrayExpression arrExpr;

    public ArrayExpression(AST.Node node)
    {
        this.arrExpr = (AST.ArrayExpression)node;
    }

    public override void generate()
    {
        arrExpr.containedType.size = arrExpr.length;
        arrExpr.containedType.generator.generate();

        LLVMTypeRef typeLLVM = typeStack.Pop();

        DebugConsole.Write("array expr type: " + typeLLVM);

        LLVMValueRef[] values = new LLVMValueRef[arrExpr.length];

        for (int i = 0; i < arrExpr.length; i++)
        {
            arrExpr.value[i].generator.generate();
            values[i] = valueStack.Pop();
            DebugConsole.Write("arr expr value: " + values[i]);
        }

        valueStack.Push(LLVM.ConstArray(typeLLVM, values));
    }
}
