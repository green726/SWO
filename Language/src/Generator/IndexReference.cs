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

    public void checkIsReference()
    {
        switch (idx.parent?.nodeType)
        {
            case AST.Node.NodeType.VariableExpression:
                AST.VariableExpression varExprPar = (AST.VariableExpression)idx.parent;
                idx.isReference = varExprPar.isReference;
                break;
            case AST.Node.NodeType.IndexReference:
                AST.IndexReference idxPar = (AST.IndexReference)idx.parent;
                idx.isReference = idxPar.isReference;
                break;
        }
    }

    public override void generate()
    {
        base.generate();
        checkIsReference();
        DebugConsole.WriteAnsi("[green]genning arr gep[/]");
        LLVMValueRef varRef = gen.valueStack.Pop();
        DebugConsole.Write("var ref: " + varRef);
        LLVMValueRef gepRef = generateGEP(varRef);
        DebugConsole.Write(gepRef);
        gen.valueStack.Push(gepRef);


        if (idx.children.Count() == 0 && !idx.isReference)
        {
            LLVMValueRef gepLoadRef = LLVM.BuildLoad(gen.builder, gepRef, "arrRefLoad");
            gen.valueStack.Push(gepLoadRef);
            DebugConsole.Write(gepLoadRef);
        }
        else
        {
            DebugConsole.WriteAnsi("[red]genning index ref children[/]");
            foreach (AST.Node child in idx.children)
            {
                child.generator.generate();
            }
        }
    }

    public LLVMValueRef generateGEP(LLVMValueRef varPtr)
    {

        LLVMTypeRef varType = varPtr.TypeOf();

        // childValueList.Add(LLVM.ConstInt(LLVMTypeRef.Int64Type(), 0, false));

        DebugConsole.Write(LLVM.GetTypeKind(varType));

        // if (LLVM.GetTypeKind(varType) != LLVMTypeKind.LLVMPointerTypeKind)
        // {
        //     int arraySize = (int)LLVM.GetArrayLength(varType);
        //     if (Config.settings.variable.arrays.outOfBoundsErrorEnabled && idx.numExpr.value > arraySize)
        //     {
        //         throw GenException.FactoryMethod("Index out of range", "Make the index in range", idx);
        //     }
        // }

        //BUG: this might need to go before the out of range checking in case there is an offset - idk
        // idx.expr.value += Config.settings.variable.arrays.startIndex;
        
        DebugConsole.Write("generating indexRef expr");
        idx.expr.generator.generate();
        DebugConsole.Write("generated indexRef expr");

        LLVMValueRef indexExpr = gen.valueStack.Pop();
        DebugConsole.Write("index expr: " + indexExpr);
        DebugConsole.Write("var ptr: " + varPtr);

        return LLVM.BuildGEP(gen.builder, varPtr, new LLVMValueRef[] { LLVM.ConstInt(LLVM.Int32Type(), 0, false), indexExpr }, "idxGEP");
        // return LLVM.BuildStructGEP(gen.builder, varPtr, (uint)idx.numExpr.value, "idxGEP");
    }
}
