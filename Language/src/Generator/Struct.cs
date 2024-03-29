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

    public void createImplValues(LLVMValueRef strPtr, string varName)
    {
        DebugConsole.Write("running create impl values in struct named: " + str.name);
        foreach (AST.StructImplement implement in str.implements)
        {
            DebugConsole.Write("creating impl values for: " + implement.trait.name);
            ((StructImplement)implement.generator).createImplValue(strPtr, varName);
        }
        DebugConsole.Write("finished running create impl values in struct named: " + str.name);
    }

    public override void generate()
    {
        base.generate();
        DebugConsole.Write("adding str with name of " + str.name + " to dict");
        gen.namedTypesAST.Add(str.name, this.str);

        List<LLVMTypeRef> elementTypes = new List<LLVMTypeRef>();

        foreach (AST.Node node in this.str.properties)
        {
            AST.VariableDeclaration varDec = (AST.VariableDeclaration)node;
            varDec.type.generator.generate();
            elementTypes.Add(gen.typeStack.Pop());
        }

        DebugConsole.Write(gen.context);

        LLVMTypeRef structType = LLVM.StructCreateNamed(LLVM.GetGlobalContext(), this.str.name);
        LLVM.StructSetBody(structType, elementTypes.ToArray(), false);

        DebugConsole.Write("struct type dump");
        DebugConsole.DumpType(structType);

        gen.typeStack.Push(structType);
        gen.namedTypesLLVM.Add(str.name, structType);

        foreach (AST.Function func in str.functions) {
            func.generator.generate();
        }

        foreach (AST.StructImplement impl in str.implements)
        {
            impl.generator.generate();
        }
    }
}
