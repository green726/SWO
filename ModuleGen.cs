using LLVMSharp;

public static class ModuleGen
{

    public static void GenerateModule(List<Parser.ASTNode> nodes)
    {
        // Make the module, which holds all the code.
        LLVMModuleRef module = LLVM.ModuleCreateWithName("HISS");
        LLVMBuilderRef builder = LLVM.CreateBuilder();
        // LLVM.LinkInMCJIT();
        // LLVM.InitializeX86TargetMC();
        // LLVM.InitializeX86Target();
        // LLVM.InitializeX86TargetInfo();
        // LLVM.InitializeX86AsmParser();
        // LLVM.InitializeX86AsmPrinter();
        // //
        // if (LLVM.CreateExecutionEngineForModule(out var engine, module, out var errorMessage).Value == 1)
        // {
        //     Console.WriteLine(errorMessage);
        //     // LLVM.DisposeMessage(errorMessage);
        //     return;
        // }
        //
        // // Create a function pass manager for this engine
        // LLVMPassManagerRef passManager = LLVM.CreateFunctionPassManagerForModule(module);
        //
        // // Set up the optimizer pipeline.  Start with registering info about how the
        // // target lays out data structures.
        // // LLVM.DisposeTargetData(LLVM.GetExecutionEngineTargetData(engine));
        //
        // // Provide basic AliasAnalysis support for GVN.
        // LLVM.AddBasicAliasAnalysisPass(passManager);
        //
        // // Promote allocas to registers.
        // LLVM.AddPromoteMemoryToRegisterPass(passManager);
        //
        // // Do simple "peephole" optimizations and bit-twiddling optzns.
        // LLVM.AddInstructionCombiningPass(passManager);
        //
        // // Reassociate expressions.
        // LLVM.AddReassociatePass(passManager);
        //
        // // Eliminate Common SubExpressions.
        // LLVM.AddGVNPass(passManager);
        //
        // // Simplify the control flow graph (deleting unreachable blocks, etc).
        // LLVM.AddCFGSimplificationPass(passManager);
        //
        // LLVM.InitializeFunctionPassManager(passManager);

        IRGen.generateIR(nodes, builder, module);


    }
}
