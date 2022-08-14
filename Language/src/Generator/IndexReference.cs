namespace Generator;

using LLVMSharp;
using Spectre.Console;
using static IRGen;

public class IndexReference : Base
{
    public AST.IndexReference idx;
    public IndexReference(AST.Node node)
    {
        this.idx = (AST.IndexReference)node;
    }

    public void checkIsPtr()
    {
        switch (idx.parent?.nodeType)
        {
            case AST.Node.NodeType.VariableExpression:
                AST.VariableExpression varExprPar = (AST.VariableExpression)idx.parent;
                idx.isPointer = varExprPar.isPointer;
                break;
            case AST.Node.NodeType.IndexReference:
                AST.IndexReference idxPar = (AST.IndexReference)idx.parent;
                idx.isPointer = idxPar.isPointer;
                break;
        }
    }

    public override void generate()
    {
        DebugConsole.WriteAnsi("[green]genning arr gep[/]");
        LLVMValueRef varRef = valueStack.Pop();
        DebugConsole.Write(varRef);
        LLVMValueRef gepRef = generateGEP(varRef);
        DebugConsole.Write(gepRef);
        valueStack.Push(gepRef);


        if (idx.children.Count() == 0)
        {
            LLVMValueRef gepLoadRef = LLVM.BuildLoad(builder, gepRef, "arrRefLoad");
            valueStack.Push(gepLoadRef);
            DebugConsole.Write(gepLoadRef);
        }

        else
        {
            foreach (AST.Node child in idx.children)
            {
                child.generator.generate();
            }
        }

        return;
    }

    public LLVMValueRef generateGEP(LLVMValueRef varPtr)
    {
        List<LLVMValueRef> childValueList = new List<LLVMValueRef>();

        LLVMTypeRef varType = varPtr.TypeOf();

        // childValueList.Add(LLVM.ConstInt(LLVMTypeRef.Int64Type(), 0, false));

        DebugConsole.Write(LLVM.GetTypeKind(varType));

        if (LLVM.GetTypeKind(varType) != LLVMTypeKind.LLVMPointerTypeKind)
        {
            int arraySize = (int)LLVM.GetArrayLength(varType);
            if (Config.settings.variable.arrays.outOfBoundsErrorEnabled && idx.numExpr.value > arraySize)
            {
                throw GenException.FactoryMethod("Index out of range", "Make the index in range", idx);
            }
        }

        //BUG: this might need to go before the out of range checking in case there is an offset - idk
        idx.numExpr.value += Config.settings.variable.arrays.startIndex;
        idx.numExpr.generator.generate();
        DebugConsole.Write(valueStack.Peek());
        childValueList.Add(valueStack.Pop());

        return LLVM.BuildStructGEP(builder, varPtr, (uint)idx.numExpr.value, "idxGEP");

    }
}
