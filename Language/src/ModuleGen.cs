using LLVMSharp;

public static class ModuleGen
{
    public static List<IRGen> CreateNewGenerators(List<Parser> parsers, Spectre.Console.ProgressTask task)
    {
        task.MaxValue = 10 * parsers.Count();

        List<IRGen> generators = new List<IRGen>();

        foreach (Parser parser in parsers)
        {
            // LLVMContextRef context = LLVM.ContextCreate();

            // Make the module, which holds all the code.
            LLVMModuleRef module = LLVM.ModuleCreateWithName(parser.fileName);
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

            IRGen gen = IRGen.addInstance(builder, module, passManager, parser, parser.fileName);
            generators.Add(gen);

        }
        // LLVM.InitializeFunctionPassManager(passManager);

        return generators;

    }

    public static List<IRGen> CreateNewGenerators(List<Parser> parsers)
    {
        List<IRGen> generators = new List<IRGen>();

        foreach (Parser parser in parsers)
        {
            // LLVMContextRef context = LLVM.ContextCreate();

            // Make the module, which holds all the code.
            LLVMModuleRef module = LLVM.ModuleCreateWithName("SWO");
            LLVMBuilderRef builder = LLVM.CreateBuilder();

            // Create a function pass manager for this engine
            LLVMPassManagerRef passManager = LLVM.CreatePassManager();

            // Provide basic AliasAnalysis support for GVN.
            LLVM.AddBasicAliasAnalysisPass(passManager);

            // Promote allocas to registers.
            LLVM.AddPromoteMemoryToRegisterPass(passManager);

            // Do simple "peephole" optimizations and bit-twiddling optzns.
            LLVM.AddInstructionCombiningPass(passManager);

            // Reassociate expressions.
            LLVM.AddReassociatePass(passManager);

            // Eliminate Common SubExpressions.
            LLVM.AddGVNPass(passManager);

            // Simplify the control flow graph (deleting unreachable blocks, etc).
            LLVM.AddCFGSimplificationPass(passManager);

            // LLVM.InitializeFunctionPassManager(passManager);
            IRGen gen = IRGen.addInstance(builder, module, passManager, parser, parser.fileName);
            generators.Add(gen);

        }

        return generators;
    }
}
