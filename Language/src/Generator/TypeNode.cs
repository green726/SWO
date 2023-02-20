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

        gen.typeStack.Push(GeneratorTypeInformation.genType((GeneratorTypeInformation)type, gen));
    }
}

