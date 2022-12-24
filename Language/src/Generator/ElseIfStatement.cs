namespace Generator;

using static IRGen;
using LLVMSharp;


public class ElseIfStatement : Base
{

    public AST.ElseIfStatement elseIf { get; set; }
    public ElseIfStatement(AST.ElseIfStatement stat)
    {
        elseIf = stat;
    }

    public override void generate()
    {
        base.generate();

        elseIf.conditional.condition.generator.generate();
        LLVMValueRef condValue = gen.valueStack.Pop();

        LLVMBasicBlockRef prevElseBlock = LLVM.GetInsertBlock(gen.builder).GetPreviousBasicBlock();

        LLVMBasicBlockRef nextBlock = LLVM.GetInsertBlock(gen.builder);

        LLVMBasicBlockRef parentBlock = LLVM.GetBasicBlockParent(prevElseBlock);

        LLVMBasicBlockRef thenBlock = LLVM.AppendBasicBlock(parentBlock, "then");

        LLVMBasicBlockRef elseBlock = LLVM.AppendBasicBlock(parentBlock, "else");

        LLVMValueRef brFromIf = LLVM.GetLastInstruction(prevElseBlock);

        LLVM.PositionBuilderAtEnd(gen.builder, elseBlock);
        LLVM.BuildBr(gen.builder, nextBlock);

        LLVM.PositionBuilderBefore(gen.builder, brFromIf);


        // brFromIf.InstructionEraseFromParent();

        LLVM.BuildCondBr(gen.builder, condValue, thenBlock, elseBlock);

        LLVM.PositionBuilderAtEnd(gen.builder, thenBlock);

        foreach (AST.Node node in elseIf.body)
        {
            node.generator.generate();
        }

        LLVM.BuildBr(gen.builder, nextBlock);

        LLVM.PositionBuilderAtEnd(gen.builder, nextBlock);

        gen.valueStack.Push(elseBlock);
        gen.valueStack.Push(nextBlock);

        LLVM.InstructionEraseFromParent(brFromIf);
    }

}
