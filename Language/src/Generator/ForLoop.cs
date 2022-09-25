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
        LLVMBasicBlockRef preHeaderBlock = LLVM.GetInsertBlock(builder);
        LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(parentBlock, "loop");

        //create the condition break
        LLVM.BuildBr(builder, loopBlock);

        LLVM.PositionBuilderAtEnd(builder, loopBlock);

        //create the phiVarDec obj for the loop
        forLoop.varDec.generator.generate();
        LLVMValueRef loopVarDec = valueStack.Pop();

        //emit the body of the loop
        foreach (AST.Node node in forLoop.body)
        {
            node.generator.generate();
        }

        DebugConsole.Write("successfully evaluated for loop body");

        //evaluate the step variable - might need to change this idk
        forLoop.loopIteration.generator.generate();
        // LLVMValueRef stepVarRef = valueStack.Pop();

        //increment the phivar by the step value
        // LLVMValueRef nextVarRef = LLVM.BuildFAdd(builder, loopVarDec, stepVarRef, "nextvar");

        //generate the LLVM binary expression for the ending condition
        forLoop.loopCondition.generator.generate();

        LLVMValueRef endCondRef = valueStack.Pop();

        // generate the post loop basic block
        LLVMBasicBlockRef endOfLoopBlock = LLVM.GetInsertBlock(builder);
        LLVMBasicBlockRef postLoopBlock = LLVM.AppendBasicBlock(parentBlock, "postloop");

        //create the condition break to evalaute where to go (ie run loop again or break out of loop)
        LLVM.BuildCondBr(builder, endCondRef, loopBlock, postLoopBlock);

        //reposition the builder
        LLVM.PositionBuilderAtEnd(builder, postLoopBlock);

        //various cleanups are below 

        //update the phivarref with the new values
        // LLVM.AddIncoming(loopVarDec, new LLVMValueRef[] { nextVarRef }, new LLVMBasicBlockRef[] { endOfLoopBlock }, 1);
        // LLVM.BuildStore(builder, nextVarRef, )

        valueStack.Push(LLVM.ConstReal(LLVMTypeRef.DoubleType(), 0));
    }
}
