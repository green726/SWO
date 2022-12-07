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

    private LLVMTypeRef genPointer()
    {
        return (LLVM.PointerType(GeneratorTypeInformation.getLLVMTypeFromString(type.value, gen, type.isArray, type.size), 0));
    }

    private LLVMTypeRef genNonPtr()
    {
        return (GeneratorTypeInformation.getLLVMTypeFromString(type.value, gen, type.isArray, type.size));
    }

    public override void generate()
    {
        base.generate();
        if (type.isPointer)
        {
            gen.typeStack.Push(genPointer());
        }
        else
        {
            gen.typeStack.Push(genNonPtr());
        }
    }
}

