using LLVMSharp;

public static class ModuleGen
{
    public static void optimizeZero(LLVMPassManagerRef passManager)
    {
        // Provide basic AliasAnalysis support for GVN.
        LLVM.AddBasicAliasAnalysisPass(passManager);

        // Promote allocas to registers.
        LLVM.AddPromoteMemoryToRegisterPass(passManager);

        // Do simple "peephole" optimizations and bit-twiddling optzns.
        LLVM.AddInstructionCombiningPass(passManager);

        // Reassociate expressions.
        LLVM.AddReassociatePass(passManager);

        // Simplify the control flow graph (deleting unreachable blocks, etc).
        LLVM.AddCFGSimplificationPass(passManager);
    }

    public static void optimizeOne(LLVMPassManagerRef passManager)
    {
        optimizeZero(passManager);

        LLVM.AddSCCPPass(passManager);

        LLVM.AddGlobalOptimizerPass(passManager);

        LLVM.AddDeadArgEliminationPass(passManager);

        LLVM.AddLoopUnrollPass(passManager);

        LLVM.AddLoopVectorizePass(passManager);

        LLVM.AddMemCpyOptPass(passManager);

        LLVM.AddLoopDeletionPass(passManager);

        LLVM.AddJumpThreadingPass(passManager);

        LLVM.AddDeadStoreEliminationPass(passManager);

        LLVM.AddLoopRotatePass(passManager);

        LLVM.AddIPSCCPPass(passManager);

        LLVM.AddLICMPass(passManager);

        LLVM.AddPruneEHPass(passManager);

        LLVM.AddLoopUnswitchPass(passManager);

        LLVM.AddAlignmentFromAssumptionsPass(passManager);

        LLVM.AddEarlyCSEPass(passManager);

        LLVM.AddStripDeadPrototypesPass(passManager);

        LLVM.AddTypeBasedAliasAnalysisPass(passManager);

        LLVM.AddScalarReplAggregatesPass(passManager);

        LLVM.AddAggressiveDCEPass(passManager);

        LLVM.AddFunctionAttrsPass(passManager);

        LLVM.AddLowerExpectIntrinsicPass(passManager);

        LLVM.AddLoopIdiomPass(passManager);

        LLVM.AddTailCallEliminationPass(passManager);

        LLVM.AddBasicAliasAnalysisPass(passManager);

        LLVM.AddIndVarSimplifyPass(passManager);

        LLVM.AddAlwaysInlinerPass(passManager);
    }

    public static void optimizeTwo(LLVMPassManagerRef passManager) {
        optimizeOne(passManager);

        LLVM.AddGlobalDCEPass(passManager);

        LLVM.AddConstantMergePass(passManager);

        LLVM.AddMergedLoadStoreMotionPass(passManager);

        LLVM.AddGVNPass(passManager);

        LLVM.AddSLPVectorizePass(passManager);

        Console.WriteLine("optimize 2 called");
    }

    public static void addOptimizations(LLVMPassManagerRef passManager, CompileCommandSettings settings)
    {
        switch (settings.optimizationLevel)
        {
            case 0:
                optimizeZero(passManager);
                break;
            case 1:
                optimizeOne(passManager);
                break;
            case 2:
                optimizeTwo(passManager);
                break;
        }
    }


    public static List<IRGen> CreateNewGenerators(List<Parser> parsers, Spectre.Console.ProgressTask task, CompileCommandSettings settings)
    {
        task.MaxValue = parsers.Count();

        List<IRGen> generators = new List<IRGen>();

        foreach (Parser parser in parsers)
        {
            // LLVMContextRef context = LLVM.ContextCreate();

            // Make the module, which holds all the code.
            LLVMModuleRef module = LLVM.ModuleCreateWithName(parser.fileName);
            LLVMBuilderRef builder = LLVM.CreateBuilder();
            // Create a function pass manager for this engine
            LLVMPassManagerRef passManager = LLVM.CreatePassManager();

            task.Increment(1);

            addOptimizations(passManager, settings);

            IRGen gen = new IRGen(builder, module, passManager, parser, parser.fileName);
            generators.Add(gen);

        }
        // LLVM.InitializeFunctionPassManager(passManager);

        return generators;

    }

    public static List<IRGen> CreateNewGenerators(List<Parser> parsers, CompileCommandSettings settings)
    {
        List<IRGen> generators = new List<IRGen>();

        // LLVMContextRef context = LLVM.ContextCreate();
        foreach (Parser parser in parsers)
        {

            // Make the module, which holds all the code.
            LLVMModuleRef module = LLVM.ModuleCreateWithName(parser.fileName);
            LLVMBuilderRef builder = LLVM.CreateBuilder();

            // Create a function pass manager for this engine
            LLVMPassManagerRef passManager = LLVM.CreatePassManager();

            addOptimizations(passManager, settings);

            // LLVM.InitializeFunctionPassManager(passManager);
            // TODO: dont add instance - just create new generator
            IRGen gen = new IRGen(builder, module, passManager, parser, parser.fileName);
            generators.Add(gen);

        }

        return generators;
    }
}
