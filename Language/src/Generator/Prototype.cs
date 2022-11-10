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
        base.generate();
        proto.handleOverload();
        DebugConsole.WriteAnsi("[yellow]proto named " + proto.name + " [/]");
        //begin argument generation
        int argumentCount = proto.arguments.Count();
        List<LLVMTypeRef> arguments = new List<LLVMTypeRef>();
        //check if function is already defined
        var function = LLVM.GetNamedFunction(gen.module, proto.name);

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
                arguments.Add(gen.typeStack.Pop());
            }

            GeneratorTypeInformation genTypeInfo = (GeneratorTypeInformation)proto.returnType;
            LLVMTypeRef retType = genTypeInfo.getLLVMType();
            LLVMTypeRef funcType = LLVM.FunctionType(gen.typeStack.Pop(), arguments.ToArray(), proto.variableArgument);
            function = LLVM.AddFunction(gen.module, proto.name, funcType);
            LLVM.SetLinkage(function, LLVMLinkage.LLVMExternalLinkage);
        }

        //NOTE: adding layer for the function
        if (proto?.parent?.nodeType == AST.Node.NodeType.Function)
        {
            gen.addLayerToNamedValueStack();
        }
        int argLoopIndex = 0;
        foreach (KeyValuePair<string, AST.Type> arg in proto.arguments)
        {
            string argumentName = arg.Key;

            LLVMValueRef param = LLVM.GetParam(function, (uint)argLoopIndex);
            LLVM.SetValueName(param, argumentName);

            if (proto?.parent?.nodeType == AST.Node.NodeType.Function)
            {
                gen.addNamedValueInScope(argumentName, param);
            }
            DebugConsole.Write("created argument with name of " + argumentName);
            argLoopIndex++;
        }

        DebugConsole.DumpValue(function);

        gen.valueStack.Push(function);
    }

}
