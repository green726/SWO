namespace Generator;
using LLVMSharp;
using static IRGen;

public class ElseStatement : Base
{
    AST.ElseStatement elseStat;

    public ElseStatement(AST.Node node)
    {
        this.elseStat = (AST.ElseStatement)node;
    }

    public override void generate()
    {
        base.generate();

        // LLVMBasicBlockRef nextBlock = LLVM.GetInsertBlock(gen.builder);
        //
        // LLVMBasicBlockRef elseBlock = LLVM.GetPreviousBasicBlock(nextBlock);

        LLVMBasicBlockRef nextBlock = gen.valueStack.Pop();
        LLVMBasicBlockRef elseBlock = gen.valueStack.Pop();

        LLVMValueRef brFromIf = LLVM.GetLastInstruction(elseBlock);

        LLVM.PositionBuilderBefore(gen.builder, brFromIf);

        DebugConsole.Write("else statement body count: " + elseStat.body.Count);

        foreach (AST.Node node in elseStat.body)
        {
            node.generator.generate();
        }
        // LLVM.BuildBr(gen.builder, nextBlock);

        LLVM.PositionBuilderAtEnd(gen.builder, nextBlock);

    }
}
