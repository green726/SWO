using LLVMSharp;

public static class ModuleGen
{
    public static void GenerateModule(List<AST.Node> nodes)
    {
        // Make the module, which holds all the code.
        LLVMModuleRef module = LLVM.ModuleCreateWithName("HISS");
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

        IRGen.generateIR(nodes, builder, module, passManager);


    }
}
