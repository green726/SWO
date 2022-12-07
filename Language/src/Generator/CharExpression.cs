namespace Generator;
using LLVMSharp;
using static IRGen;

public class CharExpression : Expression
{
    public AST.CharExpression charExpression;

    public CharExpression(AST.CharExpression charExpression) : base(charExpression)
    {
        this.charExpression = charExpression;
    }

    public override void generate()
    {
        base.generate();
        List<LLVMValueRef> asciiList = new List<LLVMValueRef>();

        bool escaped = false;
        foreach (char ch in charExpression.value)
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

        this.gen.valueStack.Push(constArrRef);

        if (charExpression.parent?.nodeType != AST.Node.NodeType.VariableDeclaration)
        {
            LLVMValueRef globalRef = LLVM.AddGlobal(gen.module, arrType, "strtmp");

            LLVM.SetInitializer(globalRef, constArrRef);
            gen.valueStack.Push(globalRef);
        }

        // DebugConsole.WriteAnsi("[red]strexpr value: " + str.value + "[/]");
        // valueStack.Push(LLVM.BuildGlobalString(builder, str.value, "strtmp"));

    }

}
