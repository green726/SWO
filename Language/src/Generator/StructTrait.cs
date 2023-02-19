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

        DebugConsole.Write("trait name: " + trait.name);
        LLVMTypeRef structType = LLVM.StructCreateNamed(LLVM.GetGlobalContext(), trait.name);

        DebugConsole.Write("struct type: " + structType);

        DebugConsole.WriteAnsi("[blue]genning struct trait[/]");
        List<LLVMTypeRef> funcTypes = new List<LLVMTypeRef>();
        foreach (AST.Prototype proto in trait.protos)
        {
            // proto.arguments.Insert(0)
            DebugConsole.Write("proto arg count: " + proto.arguments.Count);
            LLVMTypeRef[] funcArgTypes = new LLVMTypeRef[proto.arguments.Count];

            int idx = 0;
            foreach (KeyValuePair<string, AST.Type> arg in proto.arguments)
            {
                arg.Value.generator.generate();
                funcArgTypes[idx] = gen.typeStack.Pop();
                DebugConsole.Write("func args type:" + funcArgTypes[idx]);
                idx++;
            }

            funcArgTypes[0] = LLVM.PointerType(LLVM.VoidType(), 0);

            ParserTypeInformation retParserType = (ParserTypeInformation)proto.returnType;
            GeneratorTypeInformation retGenType = (GeneratorTypeInformation)retParserType;
            LLVMTypeRef funcType = LLVM.FunctionType(retGenType.genType(), funcArgTypes, false);
            DebugConsole.Write(funcType);
            funcTypes.Add(LLVM.PointerType(funcType, 0));
        }

        LLVMTypeRef traitVtable = LLVM.StructCreateNamed(LLVM.GetGlobalContext(), trait.name + "_vtable");
        LLVM.StructSetBody(traitVtable, funcTypes.ToArray(), false);
        LLVM.StructSetBody(structType, new LLVMTypeRef[] { LLVM.PointerType(LLVM.VoidType(), 0), LLVM.PointerType(traitVtable, 0) }, false);

        DebugConsole.Write(structType);
        gen.typeStack.Push(structType);
        gen.namedTypesLLVM.Add(trait.name, structType);
        gen.namedTypesLLVM.Add(trait.name + "_vtable", traitVtable);
    }
}
