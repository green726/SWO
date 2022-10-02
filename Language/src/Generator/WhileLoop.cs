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
        addLayerToNamedValueStack();
        LLVMBasicBlockRef parentBlock = LLVM.GetInsertBlock(builder).GetBasicBlockParent();

        LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(parentBlock, "whileLoopBody");
        LLVMBasicBlockRef loopCondBlock = LLVM.AppendBasicBlock(parentBlock, "whileLoopCond");
        LLVMBasicBlockRef postLoopBlock = LLVM.AppendBasicBlock(parentBlock, "whilePostLoop");

        LLVM.BuildBr(builder, loopBlock);

        LLVM.PositionBuilderAtEnd(builder, loopBlock);

        foreach (AST.Node node in loop.body)
        {
            node.generator.generate();
            DebugConsole.Write("generated node of type in for loop body: " + node.nodeType);
        }

        LLVM.BuildBr(builder, loopCondBlock);

        LLVM.PositionBuilderAtEnd(builder, loopCondBlock);

        loop.condition.generator.generate();
        LLVMValueRef condValue = valueStack.Pop();

        LLVM.BuildCondBr(builder, condValue, loopBlock, postLoopBlock);

        LLVM.PositionBuilderAtEnd(builder, postLoopBlock);
        clearNamedValueScope();


    }
}
