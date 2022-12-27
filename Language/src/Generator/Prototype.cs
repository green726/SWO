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
        DebugConsole.WriteAnsi("[yellow]proto named:[/]");
        DebugConsole.Write(proto.name);
        //begin argument generation
        int argumentCount = proto.arguments.Count();
        List<LLVMTypeRef> arguments = new List<LLVMTypeRef>();
        //check if function is already defined
        var function = LLVM.GetNamedFunction(gen.module, proto.getTrueName());

        if (function.Pointer != IntPtr.Zero)
        {
            DebugConsole.Write("Detected proto already declared");
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
            DebugConsole.Write("Detected proto NOT already declared (new proto)");
            proto.handleOverload();
            foreach (KeyValuePair<string, AST.Type> arg in proto.arguments)
            {
                DebugConsole.Write("arg type: " + arg.Value.value);
                arg.Value.generator.generate();
                DebugConsole.Write("arg: " + gen.typeStack.Peek());
                arguments.Add(gen.typeStack.Pop());
            }

            GeneratorTypeInformation genTypeInfo = (GeneratorTypeInformation)(ParserTypeInformation)proto.returnType;
            LLVMTypeRef retType = genTypeInfo.getLLVMType();
            LLVMTypeRef funcType = LLVM.FunctionType(retType, arguments.ToArray(), proto.variableArgument);
            function = LLVM.AddFunction(gen.module, proto.name, funcType);
            LLVM.SetLinkage(function, LLVMLinkage.LLVMExternalLinkage);
        }

        //NOTE: adding layer for the function
        if (proto?.parent?.nodeType == AST.Node.NodeType.Function)
        {
            gen.addLayerToNamedValueStack();
        }
        DebugConsole.Write("BREAK8");
        int argLoopIndex = 0;
        foreach (KeyValuePair<string, AST.Type> arg in proto.arguments)
        {
            string argumentName = arg.Key;
            DebugConsole.Write(argumentName);

            LLVMValueRef param = LLVM.GetParam(function, (uint)argLoopIndex);
            LLVM.SetValueName(param, argumentName);

            if (proto?.parent?.nodeType == AST.Node.NodeType.Function)
            {
                gen.addNamedValueInScope(argumentName, param);
            }
            DebugConsole.Write("created argument: " + param);
            argLoopIndex++;
        }
        DebugConsole.Write("BREAK9");

        DebugConsole.DumpValue(function);

        gen.valueStack.Push(function);
    }

}
