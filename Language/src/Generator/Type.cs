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
        // LLVMTypeRef llvmType = getBasicType();
        if (!type.isArray)
        {
            if (type.isPointer)
            {
                genPointer();
                return;
            }
            genNonArray();
        }
        else
        {
            if (type.size == null)
            {
                genPointer();
                return;
            }
            else
            {
                uint count = (uint)type.size;
                typeStack.Push(LLVM.ArrayType(getBasicType(), count));
            }
        }
    }

    public LLVMTypeRef getBasicType()
    {
        if (namedTypesLLVM.ContainsKey(type.value))
        {
            return namedTypesLLVM[type.value];
        }
        else if (type.value.StartsWith("int") || type.value.StartsWith("uint"))
        {
            uint bits = type.getIntBits();
            return LLVM.IntType(bits);
        }
        switch (type.value)
        {
            case "double":
                return LLVM.DoubleType();
            case "int":
                return LLVM.IntType(1);
            case "string":
                return LLVM.ArrayType(LLVM.Int8Type(), 3);
            case "null":
                return LLVM.VoidType();
            default:
                throw GenException.FactoryMethod("An unknown type was referenced", "Make it a known type, or remove it", this.type, true, this.type.value);
        }
    }

    public void genPointer()
    {
        typeStack.Push(LLVM.PointerType(getBasicType(), 0));
    }

    public void genNonArray()
    {
        typeStack.Push(getBasicType());
    }
}
