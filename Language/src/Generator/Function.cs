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
        if (func.generated) { return; }
        //TODO: change this in the future once more variables are added
        namedValuesLLVM.Clear();

        func.prototype.generator.generate();

        LLVMValueRef function = valueStack.Pop();
        LLVMBasicBlockRef entryBlock = LLVM.AppendBasicBlock(function, "entry");


        LLVM.PositionBuilderAtEnd(builder, entryBlock);

        // try
        // {

        if (func.prototype.name == "main")
        {
            DebugConsole.Write("main func identified");
            mainEntryBlock = entryBlock;
            mainBuilt = true;
            foreach (AST.Node node in nodesToBuild)
            {
                DebugConsole.Write("genning nodes necessary before main func");
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


        if (!this.topLevelRet)
        {
            LLVM.BuildRetVoid(builder);
        }

        LLVM.VerifyFunction(function, LLVMVerifierFailureAction.LLVMPrintMessageAction);

        valueStack.Push(function);


        // LLVM.DumpValue(function);
    }

}
