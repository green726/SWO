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

        LLVMBasicBlockRef nextBlock = LLVM.GetInsertBlock(gen.builder);

        LLVMBasicBlockRef elseBlock = LLVM.GetPreviousBasicBlock(nextBlock);

        DebugConsole.DumpValue(elseBlock);

        LLVMBasicBlockRef brBlockFromIf = LLVM.GetLastInstruction(elseBlock);

        LLVM.PositionBuilderBefore(gen.builder, brBlockFromIf);

        foreach (AST.Node node in elseStat.body)
        {
            node.generator.generate();
        }

        // LLVM.BuildBr(gen.builder, nextBlock);

        LLVM.PositionBuilderAtEnd(gen.builder, nextBlock);
    }
}
