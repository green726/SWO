namespace Generator;

using LLVMSharp;
using static IRGen;

public class WhileLoop : Base
{
    public AST.WhileLoop loop;

    public WhileLoop(AST.WhileLoop loop) : base()
    {
        this.loop = loop;
    }

    public override void generate()
    {
        gen.addLayerToNamedValueStack();
        LLVMBasicBlockRef parentBlock = LLVM.GetInsertBlock(gen.builder).GetBasicBlockParent();

        LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(parentBlock, "whileLoopBody");
        LLVMBasicBlockRef loopCondBlock = LLVM.AppendBasicBlock(parentBlock, "whileLoopCond");
        LLVMBasicBlockRef postLoopBlock = LLVM.AppendBasicBlock(parentBlock, "whilePostLoop");

        LLVM.BuildBr(gen.builder, loopBlock);

        LLVM.PositionBuilderAtEnd(gen.builder, loopBlock);

        foreach (AST.Node node in loop.body)
        {
            node.generator.generate();
            DebugConsole.Write("generated node of type in for loop body: " + node.nodeType);
        }

        LLVM.BuildBr(gen.builder, loopCondBlock);

        LLVM.PositionBuilderAtEnd(gen.builder, loopCondBlock);

        loop.condition.generator.generate();
        LLVMValueRef condValue = gen.valueStack.Pop();

        LLVM.BuildCondBr(gen.builder, condValue, loopBlock, postLoopBlock);

        LLVM.PositionBuilderAtEnd(gen.builder, postLoopBlock);
        gen.clearNamedValueScope();


    }
}
