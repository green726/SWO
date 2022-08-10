using LLVMSharp;

public static class ModuleGen
{
    public static void GenerateModule(Spectre.Console.ProgressTask task)
    {
        task.MaxValue = 10;

        LLVMContextRef context = LLVM.ContextCreate();

        // Make the module, which holds all the code.
        LLVMModuleRef module = LLVM.ModuleCreateWithName("SWO");
        task.Increment(1);
        LLVMBuilderRef builder = LLVM.CreateBuilder();
        task.Increment(1);

        // Create a function pass manager for this engine
        LLVMPassManagerRef passManager = LLVM.CreatePassManager();
        task.Increment(1);

        // Provide basic AliasAnalysis support for GVN.
        LLVM.AddBasicAliasAnalysisPass(passManager);
        task.Increment(1);

        // Promote allocas to registers.
        LLVM.AddPromoteMemoryToRegisterPass(passManager);
        task.Increment(1);

        // Do simple "peephole" optimizations and bit-twiddling optzns.
        LLVM.AddInstructionCombiningPass(passManager);
        task.Increment(1);

        // Reassociate expressions.
        LLVM.AddReassociatePass(passManager);
        task.Increment(1);

        // Eliminate Common SubExpressions.
        LLVM.AddGVNPass(passManager);
        task.Increment(1);

        // Simplify the control flow graph (deleting unreachable blocks, etc).
        LLVM.AddCFGSimplificationPass(passManager);
        task.Increment(1);

        // LLVM.InitializeFunctionPassManager(passManager);

        IRGen.initialize(builder, module, passManager, context);
        task.Increment(1);
    }
}
