namespace Generator;

using LLVMSharp;
using static IRGen;

public class IfStatement : Base
{
    AST.IfStatement ifStat;
    public bool thenTopLevelRet = false;
    public bool elseTopLevelRet = false;

    public IfStatement(AST.Node node)
    {
        this.ifStat = (AST.IfStatement)node;
    }

    public override void generate()
    {
        base.generate();
        ifStat.condition.generator.generate();
        LLVMValueRef condValue = gen.valueStack.Pop();

        // DebugConsole.Write("llvm module dump post condValue below");
        // LLVM.DumpModule(module);

        //gets the parent block (function)
        LLVMBasicBlockRef parentBlock = LLVM.GetInsertBlock(gen.builder).GetBasicBlockParent();

        LLVMBasicBlockRef thenBlock = LLVM.AppendBasicBlock(parentBlock, "then");

        LLVMBasicBlockRef elseBlock = LLVM.AppendBasicBlock(parentBlock, "else");

        LLVMBasicBlockRef nextBlock = LLVM.AppendBasicBlock(parentBlock, "next");

        LLVM.BuildCondBr(gen.builder, condValue, thenBlock, elseBlock);

        LLVM.PositionBuilderAtEnd(gen.builder, thenBlock);

        List<LLVMValueRef> thenBodyValues = new List<LLVMValueRef>();
        foreach (AST.Node node in ifStat.body)
        {
            if (node.nodeType == AST.Node.NodeType.Return)
            {
                this.thenTopLevelRet = true;
            }
            node.generator.generate();
            thenBodyValues.Add(gen.valueStack.Pop());
        }

        LLVM.BuildBr(gen.builder, nextBlock);

        //reset the then block in case builder was moved while populating it
        thenBlock = LLVM.GetInsertBlock(gen.builder);

        LLVM.PositionBuilderAtEnd(gen.builder, nextBlock);
    }

    // public void schmemerate()
    // {
    //     //evaluates the condition as a bool
    //     ifStat.condition.generator.generate();
    //     LLVMValueRef condValue = gen.valueStack.Pop();
    //
    //     // DebugConsole.Write("llvm module dump post condValue below");
    //     // LLVM.DumpModule(module);
    //
    //     //gets the parent block (function)
    //     LLVMBasicBlockRef parentBlock = LLVM.GetInsertBlock(gen.builder).GetBasicBlockParent();
    //
    //     LLVMBasicBlockRef thenBlock = LLVM.AppendBasicBlock(parentBlock, "then");
    //
    //     LLVMBasicBlockRef elseBlock = LLVM.AppendBasicBlock(parentBlock, "else");
    //
    //     LLVMBasicBlockRef mergeBlock = LLVM.AppendBasicBlock(parentBlock, "ifMerge");
    //
    //     LLVM.BuildCondBr(gen.builder, condValue, thenBlock, elseBlock);
    //
    //     // ifStat.thenFunc.generator.generate();
    //     // ifStat.elseStat.elseFunc.generator.generate();
    //
    //     //puts builder at the end of the then block to write code for it
    //     LLVM.PositionBuilderAtEnd(gen.builder, thenBlock);
    //
    //     List<LLVMValueRef> thenBodyValues = new List<LLVMValueRef>();
    //     foreach (AST.Node node in ifStat.body)
    //     {
    //         if (node.nodeType == AST.Node.NodeType.Return)
    //         {
    //             this.thenTopLevelRet = true;
    //         }
    //         node.generator.generate();
    //         thenBodyValues.Add(gen.valueStack.Pop());
    //     }
    //
    //     // ifStat.thenCall.generator.generate();
    //     // LLVMValueRef thenValRef = valueStack.Pop();
    //
    //     if (!this.thenTopLevelRet)
    //     {
    //         LLVM.BuildBr(gen.builder, mergeBlock);
    //     }
    //
    //     //reset the then block in case builder was moved while populating it
    //     thenBlock = LLVM.GetInsertBlock(gen.builder);
    //
    //     //position the builder for the else
    //     LLVM.PositionBuilderAtEnd(gen.builder, elseBlock);
    //
    //     List<LLVMValueRef> elseBodyValues = new List<LLVMValueRef>();
    //     foreach (AST.Node node in ifStat.elseStat.elseBody)
    //     {
    //         if (node.nodeType == AST.Node.NodeType.Return)
    //         {
    //             this.elseTopLevelRet = true;
    //         }
    //         node.generator.generate();
    //         thenBodyValues.Add(gen.valueStack.Pop());
    //     }
    //
    //     // ifStat.elseStat.elseCall.generator.generate();
    //     // LLVMValueRef elseValRef = valueStack.Pop();
    //
    //     if (!this.elseTopLevelRet)
    //     {
    //         LLVM.BuildBr(gen.builder, mergeBlock);
    //     }
    //
    //
    //     //resets else block
    //     elseBlock = LLVM.GetInsertBlock(gen.builder);
    //
    //     // LLVM.PositionBuilderAtEnd(builder, mergeBlock);
    //
    //     LLVM.PositionBuilderAtEnd(gen.builder, mergeBlock);
    //
    //     // LLVM.PositionBuilderAtEnd(builder, mergeBlock);
    //
    //     // LLVMValueRef phiRef = LLVM.BuildPhi(builder, LLVM.DoubleType(), "iftmp");
    //     // LLVM.AddIncoming(phiRef, new LLVMValueRef[] { thenValRef, elseValRef }, new LLVMBasicBlockRef[] { thenBlock, elseBlock }, 2);
    //
    //     // valueStack.Push(phiRef);
    //
    //     // LLVM.BuildRet(builder, phiRef);
    //
    // }
}


