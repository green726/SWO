namespace Generator;

using static IRGen;
using LLVMSharp;

public class StructImplement : Base
{
    public AST.StructImplement implement;

    private string typeName;
    private List<LLVMValueRef> funcValuesList = new List<LLVMValueRef>();

    private LLVMValueRef globalVal;

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

        List<LLVMTypeRef> funcTypesList = new List<LLVMTypeRef>();

        // funcTypesList.Add(gen.namedTypesLLVM[implement.str.name]);

        foreach (AST.Function func in implement.functions)
        {
            func.generator.generate();
            funcTypesList.Add(gen.valueStack.Peek().TypeOf());
            funcValuesList.Add(gen.valueStack.Pop());
        }

        String typeName = implement.str.name + "_" + implement.trait.name;

        // LLVMTypeRef implFuncStructType = LLVM.StructType(funcTypesList.ToArray(), false);
        LLVMTypeRef implFuncStructType = LLVM.StructCreateNamed(LLVM.GetGlobalContext(), typeName + "_vtable");
        LLVM.StructSetBody(implFuncStructType, funcTypesList.ToArray(), false);
        globalVal = LLVM.AddGlobal(gen.module, implFuncStructType, typeName + "_global");
        LLVM.SetInitializer(globalVal, LLVM.ConstStruct(funcValuesList.ToArray(), false));
        gen.namedValuesLLVM.Add(typeName + "_global", globalVal);

        LLVMTypeRef implType = LLVM.StructCreateNamed(LLVM.GetGlobalContext(), implement.str.name + "_" + implement.trait.name + "_impl");
        LLVM.StructSetBody(implType, new LLVMTypeRef[] { LLVM.PointerType(gen.namedTypesLLVM[implement.str.name], 0), LLVM.PointerType(implFuncStructType, 0) }, false);
        gen.namedTypesLLVM.Add(implement.str.name + "_" + implement.trait.name + "_impl", implType);

        LLVM.AddGlobal(gen.module, implType, "askdjaskdj");

        // gen.typeStack.Push(implFuncStructType);
        gen.namedTypesLLVM.Add(typeName + "_vtable", implFuncStructType);

        this.typeName = typeName;
    }

    public void createImplValue(LLVMValueRef strPtr, string varName)
    {
        LLVMValueRef constStruct = LLVM.ConstNamedStruct(gen.namedTypesLLVM[implement.str.name + "_" + implement.trait.name + "_impl"], new LLVMValueRef[] { strPtr, LLVM.GetNamedGlobal(gen.module, typeName + "_global") });
        LLVM.SetValueName(constStruct, varName + "_" + implement.trait.name);

        // DebugConsole.DumpValue(constStruct);

        // DebugConsole.DumpModule(gen.module);

        // throw new Exception("not implemented");


        gen.addNamedValueInScope(varName + "_" + implement.trait.name, constStruct);
    }
}
