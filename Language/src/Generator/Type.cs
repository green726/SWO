namespace Generator;
using LLVMSharp;
using static IRGen;

public class Type : Base
{
    AST.Type type;

    public Type(AST.Node node)
    {
        this.type = (AST.Type)node;
    }

    public override void generate()
    {
        switch (type.value)
        {
            case "double":
                typeStack.Push(LLVM.DoubleType());
                break;
            case "int":
                typeStack.Push(LLVM.IntType(64));
                break;
            case "string":
                typeStack.Push(LLVM.ArrayType(LLVM.Int8Type(), 3));
                break;
            case "null":
                typeStack.Push(LLVM.VoidType());
                break;

        }
    }
}
