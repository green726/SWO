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
        base.generate();
        if (func.generated) { return; }


        if (func.parent.nodeType == AST.Node.NodeType.Implement)
        {
            func.prototype.modifyThisArg();
        }
        func.prototype.generator.generate();

        LLVMValueRef function = gen.valueStack.Pop();
        LLVMBasicBlockRef entryBlock = LLVM.AppendBasicBlock(function, "entry");

        LLVM.PositionBuilderAtEnd(gen.builder, entryBlock);

        if (func.prototype.name == "main")
        {
            DebugConsole.Write("main func identified");
            gen.mainEntryBlock = entryBlock;
            gen.mainBuilt = true;
            foreach (AST.Node node in gen.nodesToBuild)
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
            LLVM.BuildRetVoid(gen.builder);
        }


        gen.valueStack.Push(function);
        gen.clearNamedValueScope();
    }

}
