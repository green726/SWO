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

    public void buildGlobalString()
    {
        AST.StringExpression strExpr = (AST.StringExpression)this.varAss.defaultValue;
        if (varAss.generated) { return; }

        List<LLVMValueRef> asciiList = new List<LLVMValueRef>();

        bool escaped = false;
        Console.WriteLine(this.varAss.defaultValue.nodeType);
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
        LLVMValueRef globalArr = LLVM.AddGlobal(module, LLVMTypeRef.ArrayType(LLVMTypeRef.Int8Type(), (uint)intsRef.Length), varAss.name);
        LLVM.SetInitializer(globalArr, arrayRef);

        valueStack.Push(globalArr);

    }

    public override void generate()
    {
        if (varAss.generated) { return; }
        if (!varAss.reassignment)
        {
            if (varAss.type.value == "string")
            {
                buildGlobalString();
                return;
            }

            this.varAss.defaultValue.generator.generate();
            LLVMValueRef valRef = valueStack.Pop();
            this.varAss.type.generator.generate();
            LLVMTypeRef typeLLVM = typeStack.Pop();

            if (!varAss.mutable)
            {
                LLVMValueRef constRef = LLVM.AddGlobal(module, typeLLVM, varAss.name);
                LLVM.SetInitializer(constRef, valRef);
                valueStack.Push(constRef);
            }
            else
            {
                if (!mainBuilt)
                {
                    // Console.WriteLine("")
                    nodesToBuild.Add(varAss);
                    return;
                }
                LLVM.PositionBuilderAtEnd(builder, mainEntryBlock);
                Console.WriteLine($"building for mutable var with name of {varAss.name} and type of");
                LLVM.DumpType(typeLLVM);
                Console.WriteLine();
                LLVMValueRef allocaRef = LLVM.BuildAlloca(builder, typeLLVM, varAss.name);
                valueStack.Push(allocaRef);
                Console.WriteLine("built and pushed alloca");
                LLVMValueRef storeRef = LLVM.BuildStore(builder, valRef, allocaRef);
                valueStack.Push(storeRef);

                namedMutablesLLVM.Add(varAss.name, allocaRef);
            }

            Console.WriteLine("adding var to named globals with name of" + varAss.name);
            namedGlobalsAST.Add(varAss.name, varAss);
        }
        else
        {
            AST.VariableAssignment originalVarAss = namedGlobalsAST[varAss.name];

            if (originalVarAss.type.value == "string")
            {
                throw new GenException("mutable strings not yet supported", varAss);
            }

            // (LLVMValueRef valRef, LLVMTypeRef typeLLVM) = generateVariableValue();


            // LLVMValueRef loadRef = LLVM.BuildLoad(builder, namedMutablesLLVM[binVarName], binVarName);
            // valueStack.Push(loadRef);
            if (varAss.binReassignment)
            {
                this.varAss.bin.generator.generate();
                LLVMValueRef binValRef = valueStack.Pop();
                LLVMValueRef storeRef = LLVM.BuildStore(builder, binValRef, namedMutablesLLVM[varAss.name]);
                valueStack.Push(storeRef);
            }
            else
            {
                Console.WriteLine(varAss?.targetValue?.nodeType);
                varAss.targetValue.generator.generate();
                LLVMValueRef targetValRef = valueStack.Pop();
                LLVMValueRef storeRef = LLVM.BuildStore(builder, targetValRef, namedMutablesLLVM[varAss.name]);
                valueStack.Push(storeRef);
            }
        }
    }


}
