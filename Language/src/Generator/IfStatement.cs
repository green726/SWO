namespace Generator;

using LLVMSharp;
using static IRGen;

public class IfStatement : Base
{
    AST.IfStatement ifStat;
    public bool thenTopLevelRet = false;
    public bool elseTopLevelRet = false;

    public LLVMBasicBlockRef elseBlock;

    public LLVMBasicBlockRef nextBlock;

    public IfStatement(AST.Node node)
    {
        this.ifStat = (AST.IfStatement)node;
    }

    public override void generate()
    {
        base.generate();
        ifStat.conditional.condition.generator.generate();
        LLVMValueRef condValue = gen.valueStack.Pop();

        // DebugConsole.Write("llvm module dump post condValue below");
        // LLVM.DumpModule(module);

        //gets the parent block (function)
        LLVMBasicBlockRef parentBlock = LLVM.GetInsertBlock(gen.builder).GetBasicBlockParent();

        LLVMBasicBlockRef thenBlock = LLVM.AppendBasicBlock(parentBlock, "then");

        LLVMBasicBlockRef nextBlock = LLVM.AppendBasicBlock(parentBlock, "next");
        this.nextBlock = nextBlock;

        if (ifStat.followingBlock.nodeType != AST.Node.NodeType.Empty)
        {
            LLVMBasicBlockRef elseBlock = LLVM.AppendBasicBlock(parentBlock, "else");
            this.elseBlock = elseBlock;
            LLVM.BuildCondBr(gen.builder, condValue, thenBlock, elseBlock);
        }
        else
        {
            LLVM.BuildCondBr(gen.builder, condValue, thenBlock, nextBlock);
        }


        LLVM.PositionBuilderAtEnd(gen.builder, thenBlock);

        foreach (AST.Node node in ifStat.body)
        {
            if (node.nodeType == AST.Node.NodeType.Return)
            {
                this.thenTopLevelRet = true;
            }
            node.generator.generate();
        }

        LLVM.BuildBr(gen.builder, nextBlock);

        //reset the then block in case builder was moved while populating it
        thenBlock = LLVM.GetInsertBlock(gen.builder);

        LLVM.PositionBuilderAtEnd(gen.builder, nextBlock);
    }

}


