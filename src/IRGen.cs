using LLVMSharp;

/*below is gep generation (prob useless)
 //below zero next to ulong is the index of the element you want to grab a pointer to
        LLVMValueRef[] arrIndices = { LLVM.ConstInt(LLVM.Int64Type(), (ulong)0, false) };
        LLVMValueRef gepRef = LLVM.BuildInBoundsGEP(builder, globalRef, arrIndices, varExp.varName);
        valueStack.Push(gepRef);
 */

public static class IRGen
{
    public static int maxStringIntLength = 64;

    public static LLVMModuleRef module;

    public static LLVMBuilderRef builder;

    public static readonly Stack<LLVMValueRef> valueStack = new Stack<LLVMValueRef>();

    public static Dictionary<string, LLVMValueRef> namedValuesLLVM = new Dictionary<string, LLVMValueRef>();

    public static Dictionary<string, VariableAssignment> namedGlobalsAST = new Dictionary<string, VariableAssignment>();

    public static void generateNumberExpression(NumberExpression numberExpression)
    {
        valueStack.Push(LLVM.ConstReal(LLVM.DoubleType(), numberExpression.value));
    }

    public static void generateStringExpression(StringExpression str)
    {
        valueStack.Push(LLVM.BuildGlobalString(builder, str.value, "strtmp"));
    }

    public static void generateVariableExpression(VariableExpression varExp)
    {
        LLVMValueRef globalRef = LLVM.GetNamedGlobal(module, varExp.varName);
        LLVMValueRef load = LLVM.BuildLoad(builder, globalRef, varExp.varName);
        valueStack.Push(load);

        if (namedGlobalsAST[varExp.varName].type.value != "string")
        {
            return;
        }

        //NOTE: below stuff doesnt seem to do anything but maybe it will so leaving it be
        LLVMValueRef[] arrIndices = { LLVM.ConstInt(LLVM.Int64Type(), (ulong)0, false) };
        LLVMValueRef gepRef = LLVM.BuildInBoundsGEP(builder, globalRef, arrIndices, varExp.varName);
        valueStack.Push(gepRef);

    }

    public static void buildGlobalString(VariableAssignment varAss)
    {

        List<LLVMValueRef> asciiList = new List<LLVMValueRef>();

        bool escaped = false;
        foreach (char ch in varAss.strValue)
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

    public static void generateVariableAssignment(VariableAssignment varAss)
    {
        namedGlobalsAST.Add(varAss.name, varAss);
        bool isString = false;
        LLVMTypeRef typeLLVM = LLVMTypeRef.DoubleType();
        LLVMValueRef constRef = LLVM.ConstReal(LLVMTypeRef.DoubleType(), 0.0);

        switch (varAss.type.value)
        {
            case "double":
                typeLLVM = LLVMTypeRef.DoubleType();
                constRef = LLVM.ConstReal(LLVMTypeRef.DoubleType(), Double.Parse(varAss.strValue));
                break;
            case "int":
                typeLLVM = LLVMTypeRef.Int64Type();
                constRef = LLVM.ConstInt(LLVMTypeRef.Int64Type(), (ulong)int.Parse(varAss.strValue), true);
                break;
            case "string":
                isString = true;
                break;

        }
        if (isString)
        {
            buildGlobalString(varAss);
            return;
        }

        LLVMValueRef varRef = LLVM.AddGlobal(module, typeLLVM, varAss.name);
        LLVM.SetInitializer(varRef, constRef);
        valueStack.Push(varRef);
    }

    public static void generateIfStatment(IfStatement ifStat)
    {



        //evaluates the condition as a bool
        generateBinaryExpression(ifStat.expression);
        LLVMValueRef condValue = valueStack.Pop();

        // Console.WriteLine("llvm module dump post condValue below");
        // LLVM.DumpModule(module);

        //gets the parent block (function)
        LLVMBasicBlockRef parentBlock = LLVM.GetInsertBlock(builder).GetBasicBlockParent();

        LLVMBasicBlockRef thenBlock = LLVM.AppendBasicBlock(parentBlock, "then");

        LLVMBasicBlockRef elseBlock = LLVM.AppendBasicBlock(parentBlock, "else");

        LLVMBasicBlockRef mergeBlock = LLVM.AppendBasicBlock(parentBlock, "ifCont");

        LLVM.BuildCondBr(builder, condValue, thenBlock, elseBlock);

        generateFunction(ifStat.thenFunc);
        generateFunction(ifStat.elseStat.elseFunc);

        //puts builder at the end of the then block to write code for it
        LLVM.PositionBuilderAtEnd(builder, thenBlock);


        generateFunctionCall(ifStat.thenCall);
        LLVMValueRef thenValRef = valueStack.Pop();


        //phi node stuff
        LLVM.BuildBr(builder, mergeBlock);

        //reset the then block in case builder was moved while populating it
        thenBlock = LLVM.GetInsertBlock(builder);


        //position the builder for the else
        LLVM.PositionBuilderAtEnd(builder, elseBlock);

        generateFunctionCall(ifStat.elseStat.elseCall);
        LLVMValueRef elseValRef = valueStack.Pop();


        LLVM.BuildBr(builder, mergeBlock);

        //resets else block
        elseBlock = LLVM.GetInsertBlock(builder);

        // LLVM.PositionBuilderAtEnd(builder, mergeBlock);



        LLVM.PositionBuilderAtEnd(builder, mergeBlock);

        LLVM.PositionBuilderAtEnd(builder, mergeBlock);

        LLVMValueRef phiRef = LLVM.BuildPhi(builder, LLVM.DoubleType(), "iftmp");
        LLVM.AddIncoming(phiRef, new LLVMValueRef[] { thenValRef, elseValRef }, new LLVMBasicBlockRef[] { thenBlock, elseBlock }, 2);

        valueStack.Push(phiRef);

        // LLVM.BuildRet(builder, phiRef);
    }

    public static void generateBinaryExpression(BinaryExpression binaryExpression)
    {
        LLVMValueRef leftHand = new LLVMValueRef();
        LLVMValueRef rightHand = new LLVMValueRef();
        LLVMValueRef ir = new LLVMValueRef();

        switch (binaryExpression.leftHand.nodeType)
        {
            case ASTNode.NodeType.NumberExpression:
                NumberExpression leftHandExpr = (NumberExpression)binaryExpression.leftHand;
                leftHand = LLVM.ConstReal(LLVM.DoubleType(), leftHandExpr.value);
                break;
            case ASTNode.NodeType.BinaryExpression:
                leftHand = valueStack.Pop();
                break;
        }

        switch (binaryExpression.rightHand.nodeType)
        {
            case ASTNode.NodeType.NumberExpression:
                NumberExpression rightHandExpr = (NumberExpression)binaryExpression.rightHand;
                rightHand = LLVM.ConstReal(LLVM.DoubleType(), rightHandExpr.value);
                break;
        }

        switch (binaryExpression.operatorType)
        {
            case BinaryExpression.OperatorType.Add:
                ir = LLVM.BuildFAdd(builder, leftHand, rightHand, "addtmp");
                break;
            case BinaryExpression.OperatorType.Equals:
                ir = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealUEQ, leftHand, rightHand, "comparetmp");
                break;
        }

        valueStack.Push(ir);

        // foreach (ASTNode child in binaryExpression.children)
        // {
        //     evaluateNode(child);
        // }

        Console.WriteLine($"Value stack peek after bin below");
        LLVM.DumpValue(valueStack.Peek());
        Console.WriteLine("");
    }

    public static StringExpression evaluatePrintFormat(FunctionCall printCall)
    {
        switch (printCall.args[0].nodeType)
        {
            case ASTNode.NodeType.NumberExpression:
                return new StringExpression(new Util.Token(Util.TokenType.String, "\"%f\"", 0, 0), printCall, true);
            case ASTNode.NodeType.StringExpression:
                return new StringExpression(new Util.Token(Util.TokenType.String, "\"%s\"", 0, 0), printCall, true);
            case ASTNode.NodeType.VariableExpression:
                VariableExpression varExpr = (VariableExpression)printCall.args[0];
                return evaluatePrintFormat(printCall, namedGlobalsAST[varExpr.varName].type);
        }

        return new StringExpression(new Util.Token(Util.TokenType.String, "\"%f\"", 0, 0), printCall, true);
    }

    public static StringExpression evaluatePrintFormat(FunctionCall printCall, TypeAST type)
    {
        switch (type.value)
        {
            case "double":
                return new StringExpression(new Util.Token(Util.TokenType.String, "\"%f\"", 0, 0), printCall, true);
            case "string":
                return new StringExpression(new Util.Token(Util.TokenType.String, "\"%s\"", 0, 0), printCall, true);
        }

        return new StringExpression(new Util.Token(Util.TokenType.String, "\"%f\"", 0, 0), printCall, true);
    }

    public static void generateBuiltinCall(FunctionCall builtIn)
    {
        StringExpression printFormat;
        switch (builtIn.functionName)
        {
            case "print":
                builtIn.functionName = "printf";

                printFormat = evaluatePrintFormat(builtIn);

                builtIn.addChildAtStart(printFormat);
                break;
            case "println":
                builtIn.functionName = "printf";

                printFormat = evaluatePrintFormat(builtIn);

                builtIn.addChildAtStart(printFormat);

                FunctionCall printNLCall = new FunctionCall(new Util.Token(Util.TokenType.Keyword, "print!", builtIn.line, builtIn.column), new List<ASTNode>() { new VariableExpression(new Util.Token(Util.TokenType.Keyword, "nl", builtIn.line, builtIn.column), parentRequired: false) }, true, builtIn.parent);
                break;
        }


        LLVMValueRef funcRef = LLVM.GetNamedFunction(module, builtIn.functionName);

        if (funcRef.Pointer == IntPtr.Zero)
        {
            throw new GenException($"Unknown function ({builtIn.functionName}) referenced", builtIn);
        }

        if (LLVM.CountParams(funcRef) != builtIn.args.Count)
        {
            throw new GenException($"Incorrect # arguments passed ({builtIn.args.Count} passed but {LLVM.CountParams(funcRef)} required)", builtIn);
        }

        int argumentCount = builtIn.args.Count;
        var argsRef = new LLVMValueRef[argumentCount];

        for (int i = 0; i < argumentCount; i++)
        {
            // Console.WriteLine("builtin with arg of: " + Parser.printASTRet(new List<ASTNode>() { builtIn.args[i] }));
            evaluateNode(builtIn.args[i]);
            argsRef[i] = valueStack.Pop();
            // Console.WriteLine(argsRef[i]);
        }

        valueStack.Push(LLVM.BuildCall(builder, funcRef, argsRef, "calltmp"));

    }

    public static void generateFunctionCall(FunctionCall funcCall)
    {
        if (funcCall.builtIn)
        {
            generateBuiltinCall(funcCall);
            return;
        }
        LLVMValueRef funcRef = LLVM.GetNamedFunction(module, funcCall.functionName);

        if (funcRef.Pointer == IntPtr.Zero)
        {
            throw new GenException($"Unknown function ({funcCall.functionName}) referenced", funcCall);
        }

        if (LLVM.CountParams(funcRef) != funcCall.args.Count)
        {
            throw new GenException($"Incorrect # arguments passed ({funcCall.args.Count} passed but {LLVM.CountParams(funcRef)} required)", funcCall);
        }

        int argumentCount = funcCall.args.Count;
        var argsRef = new LLVMValueRef[argumentCount];
        for (int i = 0; i < argumentCount; ++i)
        {
            evaluateNode(funcCall.args[i]);
            argsRef[i] = valueStack.Pop();
        }

        valueStack.Push(LLVM.BuildCall(builder, funcRef, argsRef, "calltmp"));
    }


    public static void generatePrototype(PrototypeAST prototype)
    {
        //begin argument generation
        int argumentCount = prototype.arguments.Count();
        List<LLVMTypeRef> arguments = new List<LLVMTypeRef>();
        //check if function is already defined
        var function = LLVM.GetNamedFunction(module, prototype.name);

        if (function.Pointer != IntPtr.Zero)
        {
            // If func already has a body, reject this.
            if (LLVM.CountBasicBlocks(function) != 0)
            {
                throw new GenException($"redefinition of function named {prototype.name}", prototype);
            }

            // if func originally took a different number of args, reject.
            if (LLVM.CountParams(function) != argumentCount)
            {
                throw new GenException($"redefinition of function with different number of args (redfined to: {argumentCount})", prototype);
            }
        }
        else
        {

            foreach (KeyValuePair<TypeAST, string> arg in prototype.arguments)
            {
                switch (arg.Key.value)
                {
                    case "double":
                        arguments.Add(LLVM.DoubleType());
                        break;
                    case "int":
                        arguments.Add(LLVM.IntType(64));
                        break;
                    case "string":
                        arguments.Add(LLVM.ArrayType(LLVM.Int8Type(), 3));
                        break;
                }

            }

            function = LLVM.AddFunction(module, prototype.name, LLVM.FunctionType(LLVM.DoubleType(), arguments.ToArray(), false));
            LLVM.SetLinkage(function, LLVMLinkage.LLVMExternalLinkage);

        }

        int argLoopIndex = 0;
        foreach (KeyValuePair<TypeAST, string> arg in prototype.arguments)
        {
            string argumentName = arg.Value;

            LLVMValueRef param = LLVM.GetParam(function, (uint)argLoopIndex);
            LLVM.SetValueName(param, argumentName);

            namedValuesLLVM[argumentName] = param;
        }

        valueStack.Push(function);
    }

    public static void generateFunction(FunctionAST funcNode)
    {
        //TODO: change this in the future once more variables are added
        namedValuesLLVM.Clear();

        generatePrototype(funcNode.prototype);

        LLVMValueRef function = valueStack.Pop();

        LLVM.PositionBuilderAtEnd(builder, LLVM.AppendBasicBlock(function, "entry"));

        // try
        // {
        for (var i = 0; i < funcNode.body.Count(); i++)
        {
            evaluateNode(funcNode.body[i]);
        }
        // }
        // catch (Exception)
        // {
        //     LLVM.DeleteFunction(function);
        //     throw;
        // }

        LLVM.BuildRet(builder, valueStack.Pop());

        LLVM.VerifyFunction(function, LLVMVerifierFailureAction.LLVMPrintMessageAction);

        valueStack.Push(function);
    }

    // public static void evaluateNodeRet(ASTNode node)
    // {
    //     switch (node.nodeType)
    //     {
    //         case ASTNode.NodeType.Prototype:
    //             generatePrototype((PrototypeAST)node);
    //             break;
    //         case ASTNode.NodeType.Function:
    //             generateFunction((FunctionAST)node);
    //             break;
    //         case ASTNode.NodeType.BinaryExpression:
    //             generateBinaryExpression((BinaryExpression)node);
    //             break;
    //         case ASTNode.NodeType.FunctionCall:
    //             generateFunctionCall((FunctionCall)node);
    //             break;
    //         case ASTNode.NodeType.NumberExpression:
    //             generateNumberExpression((NumberExpression)node);
    //             break;
    //         case ASTNode.NodeType.StringExpression:
    //             generateStringExpression((StringExpression)node);
    //             break;
    //         case ASTNode.NodeType.VariableAssignment:
    //             generateVariableAssignment((VariableAssignment)node);
    //             break;
    //         case ASTNode.NodeType.VariableExpression:
    //             generateVariableExpression((VariableExpression)node);
    //             break;
    //         case ASTNode.NodeType.IfStatement:
    //             generateIfStatment((IfStatement)node);
    //             break;
    //
    //     }
    // }


    public static void evaluateNode(ASTNode node)
    {
        switch (node.nodeType)
        {
            case ASTNode.NodeType.Prototype:
                generatePrototype((PrototypeAST)node);
                break;
            case ASTNode.NodeType.Function:
                generateFunction((FunctionAST)node);
                break;
            case ASTNode.NodeType.BinaryExpression:
                generateBinaryExpression((BinaryExpression)node);
                break;
            case ASTNode.NodeType.FunctionCall:
                generateFunctionCall((FunctionCall)node);
                break;
            case ASTNode.NodeType.NumberExpression:
                generateNumberExpression((NumberExpression)node);
                break;
            case ASTNode.NodeType.StringExpression:
                generateStringExpression((StringExpression)node);
                break;
            case ASTNode.NodeType.VariableAssignment:
                generateVariableAssignment((VariableAssignment)node);
                break;
            case ASTNode.NodeType.VariableExpression:
                generateVariableExpression((VariableExpression)node);
                break;
            case ASTNode.NodeType.IfStatement:
                generateIfStatment((IfStatement)node);
                break;

        }
    }

    public static void generateIR(List<ASTNode> nodes, LLVMBuilderRef _builder, LLVMModuleRef _module)
    {
        builder = _builder;
        module = _module;


        foreach (ASTNode node in nodes)
        {
            evaluateNode(node);
            Console.WriteLine("successfully evaluated node of type " + node.nodeType);

            // foreach (ASTNode child in node.children)
            // {
            //     evaluateNode(child);
            // }
            // Console.WriteLine("stack dump");
            // LLVM.DumpValue(valueStack.Peek());
        }

        Console.WriteLine("LLVM module dump below");
        LLVM.DumpModule(module);
        Console.WriteLine("");
    }

}
