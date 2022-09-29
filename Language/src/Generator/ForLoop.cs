namespace Generator;

using LLVMSharp;
using static IRGen;

public class ForLoop : Base
{
    AST.ForLoop forLoop;

    public ForLoop(AST.Node node)
    {
        this.forLoop = (AST.ForLoop)node;
    }

    public override void generate()
    {
        //TODO: replace all the phi var stuff in here with normal SWO variables (mem2reg should optimize it all into phi vars)

        //create the basic blocks for the loop
        LLVMBasicBlockRef parentBlock = LLVM.GetInsertBlock(builder).GetBasicBlockParent();
        LLVMBasicBlockRef loopConditionBlock = LLVM.AppendBasicBlock(parentBlock, "loopCond");
        LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(parentBlock, "loopBody");
        LLVMBasicBlockRef loopIncrementBlock = LLVM.AppendBasicBlock(parentBlock, "loopIncrement");
        LLVMBasicBlockRef postLoopBlock = LLVM.AppendBasicBlock(parentBlock, "postloop");

        //create the phiVarDec obj for the loop
        forLoop.varDec.generator.generate();
        LLVMValueRef loopVarDec = valueStack.Pop();

        LLVM.BuildBr(builder, loopConditionBlock);

        //loop condition checking code
        LLVM.PositionBuilderAtEnd(builder, loopConditionBlock);

        DebugConsole.Write("for loop condition generating");

        forLoop.loopCondition.generator.generate();
        LLVMValueRef endCondRef = valueStack.Pop();

        LLVM.BuildCondBr(builder, endCondRef, loopBlock, postLoopBlock);

        LLVM.PositionBuilderAtEnd(builder, loopBlock);

        //emit the body of the loop
        foreach (AST.Node node in forLoop.body)
        {
            node.generator.generate();
            DebugConsole.Write("generated node of type in for loop body: " + node.nodeType);
        }

        LLVM.BuildBr(builder, loopIncrementBlock);

        LLVM.PositionBuilderAtEnd(builder, loopIncrementBlock);

        //evaluate the step variable - might need to change this idk
        forLoop.loopIteration.generator.generate();
        LLVMValueRef iterationBin = valueStack.Pop();
        DebugConsole.WriteAnsi("Iteration bin: ");
        DebugConsole.DumpValue(iterationBin);

        LLVM.BuildBr(builder, loopConditionBlock);

        //reposition the builder
        LLVM.PositionBuilderAtEnd(builder, postLoopBlock);

    }
}
