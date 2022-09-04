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
                switch (type.value)
                {
                    case "double":
                        typeStack.Push(LLVM.ArrayType(LLVM.DoubleType(), count));
                        break;
                    case "int":
                        typeStack.Push(LLVM.ArrayType(LLVM.IntType(1), count));
                        break;
                    case "string":
                        typeStack.Push(LLVM.ArrayType(LLVM.ArrayType(LLVM.Int8Type(), 3), count));
                        break;
                    // case "null":
                    //     typeStack.Push(LLVM.VoidType());
                    //     break;
                    default:
                        throw GenException.FactoryMethod("An unknown type was referenced", "Make it a known type, or remove it", this.type, true, this.type.value);

                }

            }
        }
    }

    public void genPointer()
    {
        if (namedTypesLLVM.ContainsKey(type.value))
        {
            typeStack.Push(LLVM.PointerType(namedTypesLLVM[type.value], 0));
        }
        LLVMTypeRef typeRef = LLVMTypeRef.VoidType();
        switch (type.value)
        {
            case "double":
                typeRef = LLVM.DoubleType();
                break;
            case "int":
                typeRef = LLVM.IntType(1);
                break;
            case "string":
                typeRef = LLVM.ArrayType(LLVM.Int8Type(), 3);
                break;
            case "null":
                typeRef = LLVM.VoidType();
                break;
            default:
                throw GenException.FactoryMethod("An unknown type was referenced", "Make it a known type, or remove it", this.type, true, this.type.value);
        }
        typeStack.Push(LLVM.PointerType(typeRef, 0));
    }

    public void genNonArray()
    {
        if (namedTypesLLVM.ContainsKey(type.value))
        {
            typeStack.Push(namedTypesLLVM[type.value]);
            return;
        }
        switch (type.value)
        {
            case "double":
                typeStack.Push(LLVM.DoubleType());
                break;
            case "int":
                typeStack.Push(LLVM.IntType(1));
                break;
            case "string":
                typeStack.Push(LLVM.ArrayType(LLVM.Int8Type(), 3));
                break;
            case "null":
                typeStack.Push(LLVM.VoidType());
                break;
            default:
                throw GenException.FactoryMethod("An unknown type was referenced", "Make it a known type, or remove it", this.type, true, this.type.value);
        }
    }
}
