namespace Generator;

using static IRGen;
using LLVMSharp;

public class StructImplement : Base
{
    public AST.StructImplement implement;

    public StructImplement(AST.StructImplement implement)
    {
        this.implement = implement;
    }

    public override void generate()
    {
        base.generate();
        // implement.modifyFunctions();

        if (implement.trait.protos.Count > implement.functions.Count)
        {
            throw GenException.FactoryMethod("Not all trait functions implemented", "Implements all trait functions", implement);
        }

        else if (implement.trait.protos.Count < implement.functions.Count)
        {
            throw GenException.FactoryMethod("Too many trait functions implemented", "Implements ONLY trait functions", implement);
        }

        List<LLVMTypeRef> funcList = new List<LLVMTypeRef>();

        funcList.Add(gen.namedTypesLLVM[implement.str.name]);

        foreach (AST.Function func in implement.functions)
        {
            func.generator.generate();
            funcList.Add(gen.valueStack.Peek().TypeOf());
        }

        String typeName = implement.str.name + "_" + implement.trait.name;
        LLVMTypeRef implType = LLVM.StructCreateNamed(LLVM.GetGlobalContext(), typeName);
        LLVM.StructSetBody(implType, funcList.ToArray(), false);
        gen.typeStack.Push(implType);
        gen.namedTypesLLVM.Add(typeName, implType);

    }
}
