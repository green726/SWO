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
        //begin argument generation
        int argumentCount = proto.arguments.Count();
        List<LLVMTypeRef> arguments = new List<LLVMTypeRef>();
        //check if function is already defined
        var function = LLVM.GetNamedFunction(module, proto.name);

        if (function.Pointer != IntPtr.Zero)
        {
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

            foreach (KeyValuePair<AST.Type, string> arg in proto.arguments)
            {
                arg.Key.generator.generate();
                arguments.Add(typeStack.Pop());
            }

            function = LLVM.AddFunction(module, proto.name, LLVM.FunctionType(LLVM.DoubleType(), arguments.ToArray(), false));
            LLVM.SetLinkage(function, LLVMLinkage.LLVMExternalLinkage);

        }

        int argLoopIndex = 0;
        foreach (KeyValuePair<AST.Type, string> arg in proto.arguments)
        {
            Console.WriteLine("2nd arg loop");
            string argumentName = arg.Value;

            LLVMValueRef param = LLVM.GetParam(function, (uint)argLoopIndex);
            LLVM.SetValueName(param, argumentName);

            namedValuesLLVM[argumentName] = param;
            argLoopIndex++;
        }

        valueStack.Push(function);
    }

}
