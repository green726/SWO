namespace Generator;

using LLVMSharp;
using static IRGen;


public class Function : Base
{
    AST.Function func;

    private bool topLevelRet = false; //has a return been created as a direct child of the function?

    public Function(AST.Node node)
    {
        this.func = (AST.Function)node;
    }

    public override void generate()
    {
        Console.WriteLine("func generator called");
        if (func.generated) { return; }
        //TODO: change this in the future once more variables are added
        namedValuesLLVM.Clear();
        Console.WriteLine("hello 1");

        func.prototype.generator.generate();

        Console.WriteLine("hello 2");

        LLVMValueRef function = valueStack.Pop();
        LLVMBasicBlockRef entryBlock = LLVM.AppendBasicBlock(function, "entry");

        Console.WriteLine("hello 3");

        LLVM.PositionBuilderAtEnd(builder, entryBlock);
        func.prototype.returnType.generator.generate();
        LLVMTypeRef retType = typeStack.Pop();

        Console.WriteLine("hello 4 ret type: " + retType);

        if (retType.TypeKind != LLVMTypeKind.LLVMVoidTypeKind)
        {
            LLVMValueRef retPtr = LLVM.BuildAlloca(builder, retType, "retvalue");

            Console.WriteLine("hello 5");
            namedMutablesLLVM["retvalue"] = retPtr;

            Console.WriteLine("hello 6");

            funcReturnBlock = LLVM.AppendBasicBlock(function, "retblock");
            LLVM.PositionBuilderAtEnd(builder, funcReturnBlock);
            LLVMValueRef retPtrLoadRef = LLVM.BuildLoad(builder, retPtr, "retvalref");
            LLVM.BuildRet(builder, retPtrLoadRef);
        }
        else
        {
            funcReturnBlock = LLVM.AppendBasicBlock(function, "retblock");
            LLVM.PositionBuilderAtEnd(builder, funcReturnBlock);
            LLVM.BuildRetVoid(builder);
        }



        LLVM.PositionBuilderAtEnd(builder, entryBlock);

        // try
        // {

        if (func.prototype.name == "main")
        {
            Console.WriteLine("main func identified");
            mainEntryBlock = entryBlock;
            mainBuilt = true;
            foreach (AST.Node node in nodesToBuild)
            {
                Console.WriteLine("genning nodes necessary before main func");
                node.generator.generate();
            }
        }

        foreach (AST.Node node in func.body)
        {
            if (node.nodeType == AST.Node.NodeType.Return)
            {
                if (this.topLevelRet == true)
                {
                    GenWarning.write("Multiple top level function returns detected - this will create unreachable code", node);
                }
                this.topLevelRet = true;
            }
            node.generator.generate();
        }
        // }
        // catch (Exception)
        // {
        //     LLVM.DeleteFunction(function);
        //     throw;
        // }

        // LLVMValueRef finalRetPtrLoadRef = LLVM.BuildLoad(builder, retPtr, "retvalref");
        // LLVM.BuildRet(builder, finalRetPtrLoadRef);


        if (!this.topLevelRet)
        {
            LLVM.BuildBr(builder, funcReturnBlock);
            // LLVM.BuildRetVoid(builder);
        }

        LLVM.VerifyFunction(function, LLVMVerifierFailureAction.LLVMPrintMessageAction);

        valueStack.Push(function);

        // LLVM.DumpValue(function);
    }

}
