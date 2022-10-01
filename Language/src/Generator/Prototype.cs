namespace Generator;

using LLVMSharp;
using static IRGen;

public class Prototype : Base
{
    AST.Prototype proto;

    public Prototype(AST.Node node)
    {
        this.proto = (AST.Prototype)node;
    }

    public override void generate()
    {
        DebugConsole.WriteAnsi("[yellow]proto named " + proto.name + " [/]");
        //begin argument generation
        int argumentCount = proto.arguments.Count();
        List<LLVMTypeRef> arguments = new List<LLVMTypeRef>();
        //check if function is already defined
        var function = LLVM.GetNamedFunction(module, proto.name);

        if (function.Pointer != IntPtr.Zero)
        {
            //TODO: handle function overloading
            // If func already has a body, reject this.
            if (LLVM.CountBasicBlocks(function) != 0)
            {
                throw new GenException($"redefinition of function named {proto.name}", proto);
            }
            // if func originally took a different number of args, reject.
            if (LLVM.CountParams(function) != argumentCount)
            {
                throw new GenException($"redefinition of function with different number of args (redfined to: {argumentCount})", proto);
            }
        }
        else
        {

            foreach (KeyValuePair<string, AST.Type> arg in proto.arguments)
            {
                arg.Value.generator.generate();
                arguments.Add(typeStack.Pop());
            }

            proto.returnType.generator.generate();
            function = LLVM.AddFunction(module, proto.name, LLVM.FunctionType(typeStack.Pop(), arguments.ToArray(), false));
            LLVM.SetLinkage(function, LLVMLinkage.LLVMExternalLinkage);
        }

        //NOTE: adding layer for the function
        if (proto?.parent?.nodeType == AST.Node.NodeType.Function)
        {
            DebugConsole.WriteAnsi("[purple]KASHDKJAHSD[/]");
            addLayerToNamedValueStack();
        }
        int argLoopIndex = 0;
        foreach (KeyValuePair<string, AST.Type> arg in proto.arguments)
        {
            string argumentName = arg.Key;

            LLVMValueRef param = LLVM.GetParam(function, (uint)argLoopIndex);
            LLVM.SetValueName(param, argumentName);

            if (proto?.parent?.nodeType == AST.Node.NodeType.Function)
            {
                addNamedValueInScope(argumentName, param);
            }
            DebugConsole.Write("created argument with name of " + argumentName);
            argLoopIndex++;
        }

        valueStack.Push(function);
    }

}
