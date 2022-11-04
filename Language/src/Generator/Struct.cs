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
        base.generate();
        DebugConsole.Write("adding str with name of " + str.name + " to dict");
        gen.namedTypesAST.Add(str.name, this.str);

        foreach (AST.Node node in str.properties)
        {
            AST.VariableDeclaration varDec = (AST.VariableDeclaration)node;
            // str.propertiesNames.Add(varDec.name);
        }

        List<LLVMTypeRef> elementTypes = new List<LLVMTypeRef>();

        foreach (AST.Node node in this.str.properties)
        {
            AST.VariableDeclaration varDec = (AST.VariableDeclaration)node;
            varDec.type.generator.generate();
            elementTypes.Add(gen.typeStack.Pop());
        }


        DebugConsole.Write("ekjad");
        DebugConsole.Write(gen.context);
        //BUG: this line errors
        LLVMTypeRef structType = LLVM.StructCreateNamed(LLVM.GetGlobalContext(), this.str.name);
        LLVM.StructSetBody(structType, elementTypes.ToArray(), false);

        DebugConsole.Write("struct type dump");
        DebugConsole.DumpType(structType);

        gen.typeStack.Push(structType);
        gen.namedTypesLLVM.Add(str.name, structType);
    }
}
