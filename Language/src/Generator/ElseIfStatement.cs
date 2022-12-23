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

        LLVMBasicBlockRef thenBlock = LLVM.AppendBasicBlock(prevElseBlock, "then");

        LLVMBasicBlockRef elseBlock = LLVM.AppendBasicBlock(prevElseBlock, "else");

        LLVMBasicBlockRef nextBlock = LLVM.AppendBasicBlock(prevElseBlock, "next");

        LLVM.BuildCondBr(gen.builder, condValue, thenBlock, elseBlock);
        // LLVM.BuildCondBr(gen.builder, condValue, thenBlock, nextBlock);


        LLVM.PositionBuilderAtEnd(gen.builder, thenBlock);

        foreach (AST.Node node in elseIf.body)
        {
            node.generator.generate();
        }

        LLVM.BuildBr(gen.builder, nextBlock);

        LLVM.PositionBuilderAtEnd(gen.builder, nextBlock);
    }

}
