namespace Generator;

using LLVMSharp;
using static IRGen;

public class ForLoop
{
    AST.ForLoop forLoop;

    public ForLoop(AST.Node node)
    {
        this.forLoop = (AST.ForLoop)node;
    }

    public void generateForLoop()
    {

        //evaluate the starting value of the loop index obj
        forLoop.index.numExpr.generator.generate();
        LLVMValueRef startValRef = valueStack.Pop();

        //create the basic blocks for the loop
        LLVMBasicBlockRef parentBlock = LLVM.GetInsertBlock(builder).GetBasicBlockParent();
        LLVMBasicBlockRef preHeaderBlock = LLVM.GetInsertBlock(builder);
        LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(parentBlock, "loop");

        //create the condition break
        LLVM.BuildBr(builder, loopBlock);

        LLVM.PositionBuilderAtEnd(builder, loopBlock);

        //create the index obj for the loop
        forLoop.index.generator.generate();
        LLVMValueRef phiVarRef = valueStack.Pop();

        //initialize it with its start value
        LLVM.AddIncoming(phiVarRef, new LLVMValueRef[] { startValRef }, new LLVMBasicBlockRef[] { preHeaderBlock }, 1);

        //check if there is already a variable with this name and if so temporarily invalidate it and replace it with the phi var ref
        LLVMValueRef oldVariableRef = new LLVMValueRef();
        // VariableAssignment oldVariable = null;
        // VariableAssignment phiVarAss = new VariableAssignment(new Util.Token(Util.TokenType.Keyword, ""), false);


        bool oldVariablePresent = false;
        if (namedValuesLLVM.ContainsKey(forLoop.index.name))
        {
            oldVariableRef = namedValuesLLVM[forLoop.index.name];
            oldVariablePresent = true;
            namedValuesLLVM[forLoop.index.name] = phiVarRef;
            // namedGlobalsAST[forLoop.index.name] =
        }
        else
        {
            Console.WriteLine($"adding phiVarRef with name of {forLoop.index.name} to named values");
            namedValuesLLVM.Add(forLoop.index.name, phiVarRef);
        }

        //emit the body of the loop
        foreach (AST.Node node in forLoop.body)
        {
            node.generator.generate();
        }

        Console.WriteLine("successfully evaluated for loop body");

        //evaluate the step variable - might need to change this idk
        evaluateNode(forLoop.stepValue);
        LLVMValueRef stepVarRef = valueStack.Pop();

        //increment the phivar by the step value
        LLVMValueRef nextVarRef = LLVM.BuildFAdd(builder, phiVarRef, stepVarRef, "nextvar");

        //evaluate the end condition of the loop:

        //create a new comparison expression with the end value as the left hand
        AST.BinaryExpression endBinExpr = new AST.BinaryExpression(new Util.Token(Util.TokenType.Operator, "<", forLoop.line, forLoop.column), forLoop.index, forLoop);

        //add 0 as the right hand of the binary expression - IDK why I do this, but LLVM did so ill figure it out later
        endBinExpr.addChild(forLoop.iterationObject);

        //generate the LLVM binary expression for the ending condition
        endBinExpr.generator.generate();

        LLVMValueRef endCondRef = valueStack.Pop();

        LLVMValueRef zeroRef = LLVM.ConstReal(LLVMTypeRef.DoubleType(), 0);
        LLVMValueRef loopCondRef = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealONE, endCondRef, zeroRef, "loopcond");

        // generate the post loop basic block
        LLVMBasicBlockRef endOfLoopBlock = LLVM.GetInsertBlock(builder);
        LLVMBasicBlockRef postLoopBlock = LLVM.AppendBasicBlock(parentBlock, "postloop");

        //create the condition break to evalaute where to go (ie run loop again or break out of loop)
        LLVM.BuildCondBr(builder, loopCondRef, loopBlock, postLoopBlock);

        //reposition the builder
        LLVM.PositionBuilderAtEnd(builder, postLoopBlock);

        //various cleanups are below 

        //update the phivarref with the new values
        LLVM.AddIncoming(phiVarRef, new LLVMValueRef[] { nextVarRef }, new LLVMBasicBlockRef[] { endOfLoopBlock }, 1);

        //either replace the phiVarRef with the old variable in the vars dictionary, or remove it altogether
        if (oldVariablePresent)
        {
            namedValuesLLVM[forLoop.index.name] = oldVariableRef;
        }
        else
        {
            Console.WriteLine("removing phi var ref from named values");
            namedValuesLLVM.Remove(forLoop.index.name);
        }

        valueStack.Push(LLVM.ConstReal(LLVMTypeRef.DoubleType(), 0));

    }

}
