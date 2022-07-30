namespace Generator;
using LLVMSharp;
using static IRGen;

public class VariableDeclaration : Base
{
    public AST.VariableDeclaration varDec;

    public VariableDeclaration(AST.Node node)
    {
        this.varDec = (AST.VariableDeclaration)node;
    }

    public override void generate()
    {
        if (varDec.generated) { return; }
        if (varDec.type.value == "string")
        {
            buildGlobalString();
            return;
        }

        this.varDec.defaultValue.generator.generate();
        LLVMValueRef valRef = valueStack.Pop();
        this.varDec.type.generator.generate();
        LLVMTypeRef typeLLVM = typeStack.Pop();

        if (!varDec.mutable)
        {
            LLVMValueRef constRef = LLVM.AddGlobal(module, typeLLVM, varDec.name);
            LLVM.SetInitializer(constRef, valRef);
            valueStack.Push(constRef);
        }
        else
        {
            if (!mainBuilt)
            {
                // Console.WriteLine("")
                nodesToBuild.Add(varDec);
                return;
            }
            LLVM.PositionBuilderAtEnd(builder, mainEntryBlock);
            Console.WriteLine($"building for mutable var with name of {varDec.name} and type of");
            LLVM.DumpType(typeLLVM);
            Console.WriteLine();
            LLVMValueRef allocaRef = LLVM.BuildAlloca(builder, typeLLVM, varDec.name);
            valueStack.Push(allocaRef);
            Console.WriteLine("built and pushed alloca");
            LLVMValueRef storeRef = LLVM.BuildStore(builder, valRef, allocaRef);
            valueStack.Push(storeRef);

            namedMutablesLLVM.Add(varDec.name, allocaRef);
        }

        Console.WriteLine("adding var to named globals with name of" + varDec.name);
        namedGlobalsAST.Add(varDec.name, varDec);
    }

    public void buildGlobalString()
    {
        AST.StringExpression strExpr = (AST.StringExpression)this.varDec.defaultValue;
        if (varDec.generated) { return; }

        List<LLVMValueRef> asciiList = new List<LLVMValueRef>();

        bool escaped = false;
        Console.WriteLine(this.varDec.defaultValue.nodeType);
        foreach (char ch in strExpr.value)
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

        LLVMValueRef arrayRef = LLVM.ConstArray(LLVMTypeRef.Int8Type(), intsRef);
        LLVMValueRef globalArr = LLVM.AddGlobal(module, LLVMTypeRef.ArrayType(LLVMTypeRef.Int8Type(), (uint)intsRef.Length), varDec.name);
        LLVM.SetInitializer(globalArr, arrayRef);

        valueStack.Push(globalArr);

    }
}
