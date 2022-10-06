namespace Generator;

using LLVMSharp;
using static IRGen;

public class StringExpression : Expression
{
    AST.StringExpression str;

    public StringExpression(AST.Node node) : base((AST.Expression)node)
    {
        this.str = (AST.StringExpression)node;
    }

    public override void generate()
    {
        List<LLVMValueRef> asciiList = new List<LLVMValueRef>();

        bool escaped = false;
        foreach (char ch in str.value)
        {
            if (ch == '\\')
            {
                escaped = true;
                continue;
            }
            if (escaped)
            {
                switch (ch)
                {
                    case 'n':
                        int newLineCode = 10;
                        asciiList.Add(LLVM.ConstInt(LLVM.Int8Type(), (ulong)newLineCode, false));
                        escaped = false;
                        continue;
                }
            }
            int code = (int)ch;
            asciiList.Add(LLVM.ConstInt(LLVM.Int8Type(), (ulong)code, false));
            escaped = false;
        }
        asciiList.Add(LLVM.ConstInt(LLVM.Int8Type(), (ulong)0, false));

        LLVMValueRef[] intsRef = asciiList.ToArray();

        LLVMTypeRef arrType = LLVMTypeRef.ArrayType(LLVMTypeRef.Int8Type(), (uint)intsRef.Length);

        LLVMValueRef constArrRef = LLVM.ConstArray(LLVMTypeRef.Int8Type(), intsRef);

        gen.valueStack.Push(constArrRef);

        if (str.parent?.nodeType != AST.Node.NodeType.VariableDeclaration)
        {
            LLVMValueRef globalRef = LLVM.AddGlobal(gen.module, arrType, "strtmp");

            LLVM.SetInitializer(globalRef, constArrRef);
            gen.valueStack.Push(globalRef);
        }

        // DebugConsole.WriteAnsi("[red]strexpr value: " + str.value + "[/]");
        // valueStack.Push(LLVM.BuildGlobalString(builder, str.value, "strtmp"));

        base.generate();

    }
}
