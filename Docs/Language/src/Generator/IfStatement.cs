namespace Generator;

using LLVMSharp;
using static IRGen;

public class IfStatementDeclaration : Base
{
    AST.IfStatementDeclaration dec;

    public IfStatementDeclaration(AST.Node node)
    {
        this.dec = (AST.IfStatementDeclaration)node;
    }

    public override void generate()
    {
        dec.expression.generator.generate();
    }
}

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
        //evaluates the condition as a bool
        ifStat.declaration.generator.generate();
        LLVMValueRef condValue = valueStack.Pop();

        // DebugConsole.Write("llvm module dump post condValue below");
        // LLVM.DumpModule(module);

        //gets the parent block (function)
        LLVMBasicBlockRef parentBlock = LLVM.GetInsertBlock(builder).GetBasicBlockParent();

        LLVMBasicBlockRef thenBlock = LLVM.AppendBasicBlock(parentBlock, "then");

        LLVMBasicBlockRef elseBlock = LLVM.AppendBasicBlock(parentBlock, "else");

        LLVMBasicBlockRef mergeBlock = LLVM.AppendBasicBlock(parentBlock, "ifMerge");

        LLVM.BuildCondBr(builder, condValue, thenBlock, elseBlock);

        // ifStat.thenFunc.generator.generate();
        // ifStat.elseStat.elseFunc.generator.generate();

        //puts builder at the end of the then block to write code for it
        LLVM.PositionBuilderAtEnd(builder, thenBlock);

        List<LLVMValueRef> thenBodyValues = new List<LLVMValueRef>();
        foreach (AST.Node node in ifStat.thenBody)
        {
            if (node.nodeType == AST.Node.NodeType.Return)
            {
                this.thenTopLevelRet = true;
            }
            node.generator.generate();
            thenBodyValues.Add(valueStack.Pop());
        }

        // ifStat.thenCall.generator.generate();
        // LLVMValueRef thenValRef = valueStack.Pop();

        if (!this.thenTopLevelRet)
        {
            LLVM.BuildBr(builder, mergeBlock);
        }

        //reset the then block in case builder was moved while populating it
        thenBlock = LLVM.GetInsertBlock(builder);

        //position the builder for the else
        LLVM.PositionBuilderAtEnd(builder, elseBlock);

        List<LLVMValueRef> elseBodyValues = new List<LLVMValueRef>();
        foreach (AST.Node node in ifStat.elseStat.elseBody)
        {
            if (node.nodeType == AST.Node.NodeType.Return)
            {
                this.elseTopLevelRet = true;
            }
            node.generator.generate();
            thenBodyValues.Add(valueStack.Pop());
        }

        // ifStat.elseStat.elseCall.generator.generate();
        // LLVMValueRef elseValRef = valueStack.Pop();

        if (!this.elseTopLevelRet)
        {
            LLVM.BuildBr(builder, mergeBlock);
        }


        //resets else block
        elseBlock = LLVM.GetInsertBlock(builder);

        // LLVM.PositionBuilderAtEnd(builder, mergeBlock);

        LLVM.PositionBuilderAtEnd(builder, mergeBlock);

        // LLVM.PositionBuilderAtEnd(builder, mergeBlock);

        // LLVMValueRef phiRef = LLVM.BuildPhi(builder, LLVM.DoubleType(), "iftmp");
        // LLVM.AddIncoming(phiRef, new LLVMValueRef[] { thenValRef, elseValRef }, new LLVMBasicBlockRef[] { thenBlock, elseBlock }, 2);

        // valueStack.Push(phiRef);

        // LLVM.BuildRet(builder, phiRef);

    }
}


