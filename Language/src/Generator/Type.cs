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
        base.generate();
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
            if (type.size == 0)
            {
                genPointer();
                return;
            }
            else
            {
                uint count = (uint)type.size;
                gen.typeStack.Push(LLVM.ArrayType(getBasicType(), count));
            }
        }
    }

    public LLVMTypeRef getBasicArrayType()
    {
        string containedType = type.getContainedTypeString(type);
        DebugConsole.Write(containedType);
        if (gen.namedTypesLLVM.ContainsKey(containedType))
        {
            return LLVM.ArrayType(gen.namedTypesLLVM[containedType], (uint)type.size);
        }
        (bool isInt, int bits) = type.parser.checkInt(containedType);
        if (isInt)
        {
            return LLVM.ArrayType(LLVM.IntType((uint)bits), (uint)type.size);
        }
        switch (containedType)
        {
            case "double":
                return LLVM.ArrayType(LLVM.DoubleType(), (uint)type.size);
            case "string":
                //TODO: implement strings as stdlib so they can have a sane type
                return LLVM.ArrayType(LLVM.Int8Type(), (uint)type.size);
            case "null":
                return LLVM.ArrayType(LLVM.VoidType(), (uint)type.size);
            default:
                throw GenException.FactoryMethod("An unknown type was referenced", "Make it a known type, or remove it", this.type, true, this.type.value);
        }
    }

    public LLVMTypeRef getBasicType()
    {
        if (type.isArray)
        {
            return getBasicArrayType();
        }
        if (gen.namedTypesLLVM.ContainsKey(type.value))
        {
            return gen.namedTypesLLVM[type.value];
        }
        (bool isInt, int bits) = type.parser.checkInt(type.value);
        if (isInt)
        {
            return LLVM.IntType((uint)bits);
        }
        switch (type.value)
        {
            case "double":
                return LLVM.DoubleType();
            case "string":
                //TODO: implement strings as stdlib so they can have a sane type
                return LLVM.ArrayType(LLVM.Int8Type(), 0);
            case "null":
                return LLVM.VoidType();
            default:
                throw GenException.FactoryMethod("An unknown type was referenced", "Make it a known type, or remove it", this.type, true, this.type.value);
        }
    }

    public void genPointer()
    {
        gen.typeStack.Push(LLVM.PointerType(getBasicType(), 0));
    }

    public void genNonArray()
    {
        gen.typeStack.Push(getBasicType());
    }
}
