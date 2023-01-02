namespace Generator;
using static IRGen;
using LLVMSharp;

public class StructTrait : Base
{
    public AST.StructTrait trait;
    public StructTrait(AST.StructTrait trait)
    {
        this.trait = trait;
    }

    public override void generate()
    {
        base.generate();

        DebugConsole.WriteAnsi("[blue]genning struct trait[/]");
        List<LLVMTypeRef> funcTypes = new List<LLVMTypeRef>();
        foreach (AST.Prototype proto in trait.protos)
        {
            LLVMTypeRef[] funcArgTypes = new LLVMTypeRef[proto.arguments.Count];

            int idx = 0;
            foreach (KeyValuePair<string, AST.Type> arg in proto.arguments)
            {
                arg.Value.generator.generate();
                funcArgTypes[idx] = gen.typeStack.Pop();
                idx++;
            }
            ParserTypeInformation retParserType = (ParserTypeInformation)proto.returnType;
            GeneratorTypeInformation retGenType = (GeneratorTypeInformation)retParserType;
            LLVMTypeRef funcType = LLVM.FunctionType(retGenType.getLLVMType(), funcArgTypes, false);
            funcTypes.Add(funcType);
        }

        DebugConsole.Write("trait name: " + trait.name);
        LLVMTypeRef structType = LLVM.StructCreateNamed(LLVM.GetGlobalContext(), trait.name);

        DebugConsole.Write("struct type: " + structType);

        LLVM.StructSetBody(structType, funcTypes.ToArray(), false);

        gen.typeStack.Push(structType);
        gen.namedTypesLLVM.Add(trait.name, structType);
    }
}
