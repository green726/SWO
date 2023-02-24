namespace Generator;

using LLVMSharp;
using static IRGen;

public class VariableAssignment : Base
{
    AST.VariableAssignment varAss;

    public VariableAssignment(AST.Node node)
    {
        this.varAss = (AST.VariableAssignment)node;
    }

    public override void generate()
    {
        base.generate();

        //BUG: might need to add back in the below if statement | I HATE LLVM
        // if (!varAss.type.isStruct)
        // {
        varAss.varExpr.isReference = true;
        // }
        varAss.varExpr.generator.generate();
        LLVMValueRef targetValRef = gen.valueStack.Pop();
        // LLVMValueRef targetValRef = gen.getNamedValueInScope(varAss.varExpr.value);
        if (varAss.binReassignment)
        {
            DebugConsole.WriteAnsi("[green]bin reass detected[/]");
            this?.varAss?.bin?.generator.generate();
            LLVMValueRef binValRef = gen.valueStack.Pop();
            LLVMValueRef storeRef = LLVM.BuildStore(gen.builder, binValRef, targetValRef);
            gen.valueStack.Push(storeRef);
        }
        else
        {
            DebugConsole.WriteAnsi("[green]non bin reass detected[/]");
            DebugConsole.Write(varAss.targetValue);
            varAss.targetValue.generator.generate();
            DebugConsole.Write("stack peek: " + gen.valueStack.Peek());
            LLVMValueRef resultValRef = gen.valueStack.Pop();
            LLVMValueRef storeRef = LLVM.BuildStore(gen.builder, resultValRef, targetValRef);
            gen.valueStack.Push(storeRef);
        }
    }
}
