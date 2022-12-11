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



    }
}
