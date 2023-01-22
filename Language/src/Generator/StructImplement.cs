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

        // foreach (AST.Function func in implement.functions)
        // {
        //     // func.prototype.arguments["this"] = new GeneratorTypeInformation("void*", implement.parser);
        //     func.prototype.modifyThisArg();
        //
        //     Prototype protoGen = (Prototype)func.prototype.generator;
        //     funcTypesList.Add(protoGen.getType());
        // }

        typeName = implement.str.name + "_" + implement.trait.name;
        //
        // // LLVMTypeRef implFuncStructType = LLVM.StructType(funcTypesList.ToArray(), false);
        // LLVMTypeRef implFuncStructType = LLVM.StructCreateNamed(LLVM.GetGlobalContext(), typeName + "_vtable");
        // LLVM.StructSetBody(implFuncStructType, funcTypesList.ToArray(), false);
        //
        //
        // LLVMTypeRef implType = LLVM.StructCreateNamed(LLVM.GetGlobalContext(), implement.str.name + "_" + implement.trait.name + "_impl");
        // LLVM.StructSetBody(implType, new LLVMTypeRef[] { LLVM.PointerType(LLVM.VoidType(), 0), LLVM.PointerType(implFuncStructType, 0) }, false);
        // gen.namedTypesLLVM.Add(implement.str.name + "_" + implement.trait.name + "_impl", implType);

        // LLVM.AddGlobal(gen.module, implType, "askdjaskdj");

        // gen.typeStack.Push(implFuncStructType);
        // gen.namedTypesLLVM.Add(typeName + "_vtable", implFuncStructType);

        globalVal = LLVM.AddGlobal(gen.module, gen.namedTypesLLVM[implement.trait.name + "_vtable"], typeName + "_global");

        foreach (AST.Function func in implement.functions)
        {
            func.generator.generate();
            funcValuesList.Add(gen.valueStack.Pop());
        }

        LLVM.SetInitializer(globalVal, LLVM.ConstStruct(funcValuesList.ToArray(), false));
        gen.namedValuesLLVM.Add(typeName + "_global", globalVal);

        DebugConsole.WriteAnsi("[red]typename: " + typeName + "[/]");
    }

    public void createImplValue(LLVMValueRef strPtr, string varName)
    {
        DebugConsole.WriteAnsi("[red]typename (in create impl value): " + typeName + " with str ptr: " + strPtr + "[/]");
        // LLVMValueRef bitCast = LLVM.BuildCast(gen.builder, LLVMOpcode.LLVMBitCast, strPtr, LLVM.PointerType(LLVM.VoidType(), 0), "voidPtrCast");
        // LLVMValueRef constStruct = LLVM.ConstNamedStruct(gen.namedTypesLLVM[implement.trait.name], new LLVMValueRef[] { bitCast, LLVM.GetNamedGlobal(gen.module, typeName + "_global") });
        // LLVM.SetValueName(constStruct, varName + "_" + implement.trait.name);


        LLVMValueRef allocaRef = LLVM.BuildAlloca(gen.builder, gen.namedTypesLLVM[implement.trait.name], varName + "_" + implement.trait.name);
        DebugConsole.Write(allocaRef);
        DebugConsole.Write(LLVM.GetNamedGlobal(gen.module, typeName + "_global"));

        LLVMValueRef gepForStructItself = LLVM.BuildStructGEP(gen.builder, allocaRef, 0, "gepForStructItself");
        LLVMValueRef gepForVtable = LLVM.BuildStructGEP(gen.builder, allocaRef, 1, "gepForVtable");

        LLVMValueRef storeForStructItself = LLVM.BuildStore(gen.builder, strPtr, gepForStructItself);
        LLVMValueRef storeForVtable = LLVM.BuildStore(gen.builder, LLVM.GetNamedGlobal(gen.module, typeName + "_global"), gepForVtable);

        gen.addNamedValueInScope(varName + "_" + implement.trait.name, allocaRef);
        // gen.addNamedValueInScope(varName + "_" + implement.trait.name, constStruct);
    }
}
