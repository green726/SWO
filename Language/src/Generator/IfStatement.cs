namespace Generator;

using LLVMSharp;
using static IRGen;

public class IfStatement : Base
{
    AST.IfStatement ifStat;

    public IfStatement(AST.Node node)
    {
        this.ifStat = (AST.IfStatement)node;
    }

    public override void generate()
    {
        //evaluates the condition as a bool
        ifStat.expression.generator.generate();
        LLVMValueRef condValue = valueStack.Pop();

        // Console.WriteLine("llvm module dump post condValue below");
        // LLVM.DumpModule(module);

        //gets the parent block (function)
        LLVMBasicBlockRef parentBlock = LLVM.GetInsertBlock(builder).GetBasicBlockParent();

        LLVMBasicBlockRef thenBlock = LLVM.AppendBasicBlock(parentBlock, "then");

        LLVMBasicBlockRef elseBlock = LLVM.AppendBasicBlock(parentBlock, "else");

        LLVMBasicBlockRef mergeBlock = LLVM.AppendBasicBlock(parentBlock, "ifCont");

        LLVM.BuildCondBr(builder, condValue, thenBlock, elseBlock);

        ifStat.thenFunc.generator.generate();
        ifStat.elseStat.elseFunc.generator.generate();

        //puts builder at the end of the then block to write code for it
        LLVM.PositionBuilderAtEnd(builder, thenBlock);


        ifStat.thenCall.generator.generate();
        LLVMValueRef thenValRef = valueStack.Pop();


        //phi node stuff
        LLVM.BuildBr(builder, mergeBlock);

        //reset the then block in case builder was moved while populating it
        thenBlock = LLVM.GetInsertBlock(builder);


        //position the builder for the else
        LLVM.PositionBuilderAtEnd(builder, elseBlock);

        ifStat.elseStat.elseCall.generator.generate();
        LLVMValueRef elseValRef = valueStack.Pop();


        LLVM.BuildBr(builder, mergeBlock);

        //resets else block
        elseBlock = LLVM.GetInsertBlock(builder);

        // LLVM.PositionBuilderAtEnd(builder, mergeBlock);



        LLVM.PositionBuilderAtEnd(builder, mergeBlock);

        LLVM.PositionBuilderAtEnd(builder, mergeBlock);

        LLVMValueRef phiRef = LLVM.BuildPhi(builder, LLVM.DoubleType(), "iftmp");
        LLVM.AddIncoming(phiRef, new LLVMValueRef[] { thenValRef, elseValRef }, new LLVMBasicBlockRef[] { thenBlock, elseBlock }, 2);

        valueStack.Push(phiRef);

        // LLVM.BuildRet(builder, phiRef);

    }
}


