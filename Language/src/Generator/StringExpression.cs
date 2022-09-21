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
        // if (str.builtInString)
        // {
        //     switch (str.value)
        //     {
        //         case "\"%s\"":
        //             LLVMValueRef stringFormatRef = LLVM.GetNamedGlobal(module, "stringFormat");
        //             if (stringFormatRef.Pointer == IntPtr.Zero)
        //             {
        //                 valueStack.Push(LLVM.BuildGlobalString(builder, str.value, "stringFormat"));
        //             }
        //             else
        //             {
        //                 valueStack.Push(stringFormatRef);
        //             }
        //             break;
        //         case "\"%f\"":
        //             LLVMValueRef numberFormatRef = LLVM.GetNamedGlobal(module, "numberFormat");
        //             if (numberFormatRef.Pointer == IntPtr.Zero)
        //             {
        //                 valueStack.Push(LLVM.BuildGlobalString(builder, str.value, "numberFormat"));
        //             }
        //             else
        //             {
        //                 valueStack.Push(numberFormatRef);
        //             }
        //             break;
        //         case "\"\n\"":
        //             LLVMValueRef newLineRef = LLVM.GetNamedGlobal(module, "newLine");
        //             if (newLineRef.Pointer == IntPtr.Zero)
        //             {
        //                 valueStack.Push(LLVM.BuildGlobalString(builder, str.value, "newLine"));
        //             }
        //             else
        //             {
        //                 valueStack.Push(newLineRef);
        //             }
        //             break;
        //     }
        //
        // }
        // else
        // {
            valueStack.Push(LLVM.BuildGlobalString(builder, str.value, "strtmp"));
        // }

        base.generate();

    }
}
