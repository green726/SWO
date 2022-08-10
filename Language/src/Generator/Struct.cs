namespace Generator;

using LLVMSharp;
using static IRGen;

public class Struct : Base
{
    AST.Struct str;
    public Struct(AST.Node node)
    {
        this.str = (AST.Struct)node;
    }

    public override void generate()
    {
        List<LLVMTypeRef> elementTypes = new List<LLVMTypeRef>();

        foreach (AST.Node node in this.str.properties)
        {
            AST.VariableDeclaration varDec = (AST.VariableDeclaration)node;
            varDec.type.generator.generate();
            elementTypes.Add(typeStack.Pop());
        }

        LLVMTypeRef structType = LLVM.StructCreateNamed(context, this.str.name);
        LLVM.StructSetBody(structType, elementTypes.ToArray(), false);

        Console.WriteLine("struct type dump");
        LLVM.DumpType(structType);

        typeStack.Push(structType);
    }
}
