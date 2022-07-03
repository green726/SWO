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

    public static LLVMPassManagerRef passManager;

    public static readonly Stack<LLVMValueRef> valueStack = new Stack<LLVMValueRef>();

    public static Dictionary<string, LLVMValueRef> namedValuesLLVM = new Dictionary<string, LLVMValueRef>();

    public static Dictionary<string, VariableAssignment> namedGlobalsAST = new Dictionary<string, VariableAssignment>();

    public static LLVMBasicBlockRef mainEntryBlock;
    public static bool mainBuilt = false;
    public static List<ASTNode> nodesToBuild = new List<ASTNode>();

    public static Dictionary<string, LLVMValueRef> namedMutablesLLVM = new Dictionary<string, LLVMValueRef>();

    public static void generateNumberExpression(NumberExpression numberExpression)
    {
        valueStack.Push(LLVM.ConstReal(LLVM.DoubleType(), numberExpression.value));
    }

    public static void generateStringExpression(StringExpression str)
    {
        if (str.builtInString)
        {
            switch (str.value)
            {
                case "\"%s\"":
                    LLVMValueRef stringFormatRef = LLVM.GetNamedGlobal(module, "stringFormat");
                    if (stringFormatRef.Pointer == IntPtr.Zero)
                    {
                        valueStack.Push(LLVM.BuildGlobalString(builder, str.value, "stringFormat"));
                    }
                    else
                    {
                        valueStack.Push(stringFormatRef);
                    }
                    break;
                case "\"%f\"":
                    LLVMValueRef numberFormatRef = LLVM.GetNamedGlobal(module, "numberFormat");
                    if (numberFormatRef.Pointer == IntPtr.Zero)
                    {
                        valueStack.Push(LLVM.BuildGlobalString(builder, str.value, "numberFormat"));
                    }
                    else
                    {
                        valueStack.Push(numberFormatRef);
                    }
                    break;
                case "\"\n\"":
                    LLVMValueRef newLineRef = LLVM.GetNamedGlobal(module, "newLine");
                    if (newLineRef.Pointer == IntPtr.Zero)
                    {
                        valueStack.Push(LLVM.BuildGlobalString(builder, str.value, "newLine"));
                    }
                    else
                    {
                        valueStack.Push(newLineRef);
                    }
                    break;
            }

        }
        else
        {
            valueStack.Push(LLVM.BuildGlobalString(builder, str.value, "strtmp"));
        }
    }

    public static void generateVariableExpression(VariableExpression varExpr)
    {
        LLVMValueRef varRef = LLVM.GetNamedGlobal(module, varExpr.varName);
        if (varRef.Pointer == IntPtr.Zero)
        {
            Console.WriteLine("var ref was pointer zero");
            if (namedMutablesLLVM.ContainsKey(varExpr.varName))
            {
                //code to load a stack mut
                valueStack.Push(LLVM.BuildLoad(builder, namedMutablesLLVM[varExpr.varName], varExpr.varName));
                return;
            }
            else
            {
                varRef = namedValuesLLVM[varExpr.varName];
                if (varRef.Pointer != IntPtr.Zero)
                {
                    valueStack.Push(varRef);
                    return;
                }
            }

            throw GenException.FactoryMethod("An unknown variable was referenced", "Likely a typo", varExpr, true, varExpr.varName);
        }
        else
        {
            LLVMValueRef load = LLVM.BuildLoad(builder, varRef, varExpr.varName);
            valueStack.Push(load);
            return;
        }


        // if (namedGlobalsAST[varExpr.varName].type.value != "string")
        // {
        //     return;
        // }

        //NOTE: below stuff doesnt seem to do anything but maybe it will so leaving it be
        // LLVMValueRef[] arrIndices = { LLVM.ConstInt(LLVM.Int64Type(), (ulong)0, false) };
        // LLVMValueRef gepRef = LLVM.BuildInBoundsGEP(builder, globalRef, arrIndices, varExp.varName);
        // valueStack.Push(gepRef);

    }

    public static void generateVariableExpression(PhiVariable varExpr)
    {
        LLVMValueRef varRef = namedValuesLLVM[varExpr.name];
        if (varRef.Pointer != IntPtr.Zero)
        {
            valueStack.Push(varRef);
            return;
        }
        else
        {
            throw new GenException($"could not find local phi variable named {varExpr.name}", varExpr);
        }
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

    public static (LLVMValueRef, LLVMTypeRef) generateVariableValue(VariableAssignment varAss)
    {
        LLVMTypeRef typeLLVM = LLVMTypeRef.DoubleType();
        LLVMValueRef valRef = LLVM.ConstReal(LLVMTypeRef.DoubleType(), 0.0);
        switch (varAss.type.value)
        {
            case "double":
                typeLLVM = LLVMTypeRef.DoubleType();
                valRef = LLVM.ConstReal(LLVMTypeRef.DoubleType(), Double.Parse(varAss.strValue));
                break;
            case "int":
                typeLLVM = LLVMTypeRef.Int64Type();
                valRef = LLVM.ConstInt(LLVMTypeRef.Int64Type(), (ulong)int.Parse(varAss.strValue), true);
                break;
        }
        return (valRef, typeLLVM);
    }

    public static void generateVariableAssignment(VariableAssignment varAss)
    {
        if (!varAss.reassignment)
        {
            if (varAss.type.value == "string")
            {
                buildGlobalString(varAss);
                return;
            }

            (LLVMValueRef valRef, LLVMTypeRef typeLLVM) = generateVariableValue(varAss);

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

            namedGlobalsAST.Add(varAss.name, varAss);
        }
        else
        {
            VariableAssignment originalVarAss = namedGlobalsAST[varAss.name];

            if (originalVarAss.type.value == "string")
            {
                throw new GenException("mutable strings not yet supported", varAss);
            }

            (LLVMValueRef valRef, LLVMTypeRef typeLLVM) = generateVariableValue(originalVarAss);


            // LLVMValueRef loadRef = LLVM.BuildLoad(builder, namedMutablesLLVM[binVarName], binVarName);
            // valueStack.Push(loadRef);
            if (varAss.binReassignment)
            {
                generateBinaryExpression(varAss.bin);
                LLVMValueRef binValRef = valueStack.Pop();
                LLVMValueRef storeRef = LLVM.BuildStore(builder, binValRef, namedMutablesLLVM[varAss.name]);
                valueStack.Push(storeRef);
            }
            else
            {
                evaluateNode(varAss.targetValue);
                LLVMValueRef targetValRef = valueStack.Pop();
                LLVMValueRef storeRef = LLVM.BuildStore(builder, targetValRef, namedMutablesLLVM[varAss.name]);
                valueStack.Push(storeRef);
            }
        }
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
            case ASTNode.NodeType.VariableExpression:
                VariableExpression leftHandVarExpr = (VariableExpression)binaryExpression.leftHand;
                Console.WriteLine("generating bin expr with lhs of var expr that has name of " + leftHandVarExpr.varName);
                evaluateNode(binaryExpression.leftHand);
                leftHand = valueStack.Pop();
                break;
            case ASTNode.NodeType.NumberExpression:
                NumberExpression leftHandExpr = (NumberExpression)binaryExpression.leftHand;
                Console.WriteLine("bin expr left hand num expr with value of " + leftHandExpr.value);
                leftHand = LLVM.ConstReal(LLVM.DoubleType(), leftHandExpr.value);
                LLVM.DumpValue(leftHand);
                break;
            case ASTNode.NodeType.BinaryExpression:
                leftHand = valueStack.Pop();
                break;
            case ASTNode.NodeType.PhiVariable:

                generateVariableExpression((PhiVariable)binaryExpression.leftHand);
                leftHand = valueStack.Pop();
                break;
        }

        switch (binaryExpression.rightHand.nodeType)
        {
            case ASTNode.NodeType.VariableExpression:
                VariableExpression rightHandVarExpr = (VariableExpression)binaryExpression.rightHand;
                Console.WriteLine("generating bin expr with rhs of var expr that has name of " + rightHandVarExpr.varName);
                evaluateNode(binaryExpression.rightHand);
                rightHand = valueStack.Pop();
                break;
            case ASTNode.NodeType.NumberExpression:
                NumberExpression rightHandExpr = (NumberExpression)binaryExpression.rightHand;
                rightHand = LLVM.ConstReal(LLVM.DoubleType(), rightHandExpr.value);
                break;
            case ASTNode.NodeType.PhiVariable:
                generateVariableExpression((PhiVariable)binaryExpression.rightHand);
                rightHand = valueStack.Pop();
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
            case BinaryExpression.OperatorType.LessThan:
                Console.WriteLine("left hand value dump below");
                LLVM.DumpValue(leftHand);
                Console.WriteLine();
                LLVMValueRef cmpRef = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealULT, leftHand, rightHand, "comparetmp");
                ir = LLVM.BuildUIToFP(builder, cmpRef, LLVMTypeRef.DoubleType(), "booltmp");
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

    public static TypeAST LLVMTypeToASTType(LLVMTypeRef type, ASTNode parent)
    {
        Console.WriteLine($"Converting llvm type with kind of {type.TypeKind}");
        switch (type.TypeKind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
                return new TypeAST(new Util.Token(Util.TokenType.Keyword, "double", parent.line, parent.column));
        }

        return null;
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
                if (namedGlobalsAST.ContainsKey(varExpr.varName))
                {
                    return evaluatePrintFormat(printCall, namedGlobalsAST[varExpr.varName].type);
                }
                else if (namedValuesLLVM.ContainsKey(varExpr.varName))
                {
                    TypeAST printType = LLVMTypeToASTType(namedValuesLLVM[varExpr.varName].TypeOf(), printCall);
                    return evaluatePrintFormat(printCall, printType);
                }
                throw GenException.FactoryMethod("An unknown variable was printed", "Likely a typo", varExpr, true, varExpr.varName);


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
            default:
                throw new GenException($"attempting to print obj of unknown type | obj: {printCall.args[0]} type: {type}", printCall);
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
                // Console.WriteLine("successfully evaluated print format");

                builtIn.addChildAtStart(printFormat);
                // Console.WriteLine("appended child to start of print call");
                break;
            case "println":
                builtIn.functionName = "printf";

                printFormat = evaluatePrintFormat(builtIn);
                Console.WriteLine("successfully evaluated print format");

                builtIn.addChildAtStart(printFormat);

                FunctionCall printNLCall = new FunctionCall(new Util.Token(Util.TokenType.Keyword, "print!", builtIn.line, builtIn.column), new List<ASTNode>() { new VariableExpression(new Util.Token(Util.TokenType.Keyword, "nl", builtIn.line, builtIn.column), parentRequired: false) }, true, builtIn.parent, false);
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
            // Console.WriteLine($"evaluated builtin arg of {builtIn.args[i]}");
        }

        valueStack.Push(LLVM.BuildCall(builder, funcRef, argsRef, "calltmp"));
        // Console.WriteLine("successfully evaluated builtin call");

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
        LLVMBasicBlockRef entryBlock = LLVM.AppendBasicBlock(function, "entry");

        LLVM.PositionBuilderAtEnd(builder, entryBlock);

        // try
        // {

        if (funcNode.prototype.name == "main")
        {
            Console.WriteLine("main func identified");
            mainEntryBlock = entryBlock;
            mainBuilt = true;
            foreach (ASTNode node in nodesToBuild)
            {
                evaluateNode(node);
            }
        }

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

    public static void generateForLoop(ForLoop forLoop)
    {

        //evaluate the starting value of the loop index obj
        generateNumberExpression(forLoop.index.numExpr);
        LLVMValueRef startValRef = valueStack.Pop();

        //create the basic blocks for the loop
        LLVMBasicBlockRef parentBlock = LLVM.GetInsertBlock(builder).GetBasicBlockParent();
        LLVMBasicBlockRef preHeaderBlock = LLVM.GetInsertBlock(builder);
        LLVMBasicBlockRef loopBlock = LLVM.AppendBasicBlock(parentBlock, "loop");

        //create the condition break
        LLVM.BuildBr(builder, loopBlock);

        LLVM.PositionBuilderAtEnd(builder, loopBlock);

        //create the index obj for the loop
        generatePhiVar(forLoop.index);
        LLVMValueRef phiVarRef = valueStack.Pop();

        //initialize it with its start value
        LLVM.AddIncoming(phiVarRef, new LLVMValueRef[] { startValRef }, new LLVMBasicBlockRef[] { preHeaderBlock }, 1);

        //check if there is already a variable with this name and if so temporarily invalidate it and replace it with the phi var ref
        LLVMValueRef oldVariableRef = new LLVMValueRef();
        // VariableAssignment oldVariable = null;
        // VariableAssignment phiVarAss = new VariableAssignment(new Util.Token(Util.TokenType.Keyword, ""), false);


        bool oldVariablePresent = false;
        if (namedValuesLLVM.ContainsKey(forLoop.index.name))
        {
            oldVariableRef = namedValuesLLVM[forLoop.index.name];
            oldVariablePresent = true;
            namedValuesLLVM[forLoop.index.name] = phiVarRef;
            // namedGlobalsAST[forLoop.index.name] =
        }
        else
        {
            Console.WriteLine($"adding phiVarRef with name of {forLoop.index.name} to named values");
            namedValuesLLVM.Add(forLoop.index.name, phiVarRef);
        }

        //emit the body of the loop
        foreach (ASTNode node in forLoop.body)
        {
            evaluateNode(node);
        }

        Console.WriteLine("successfully evaluated for loop body");

        //evaluate the step variable - might need to change this idk
        evaluateNode(forLoop.stepValue);
        LLVMValueRef stepVarRef = valueStack.Pop();

        //increment the phivar by the step value
        LLVMValueRef nextVarRef = LLVM.BuildFAdd(builder, phiVarRef, stepVarRef, "nextvar");

        //evaluate the end condition of the loop:

        //create a new comparison expression with the end value as the left hand
        BinaryExpression endBinExpr = new BinaryExpression(new Util.Token(Util.TokenType.Operator, "<", forLoop.line, forLoop.column), forLoop.index, forLoop);

        //add 0 as the right hand of the binary expression - IDK why I do this, but LLVM did so ill figure it out later
        endBinExpr.addChild(forLoop.iterationObject);

        //generate the LLVM binary expression for the ending condition
        generateBinaryExpression(endBinExpr);

        LLVMValueRef endCondRef = valueStack.Pop();

        LLVMValueRef zeroRef = LLVM.ConstReal(LLVMTypeRef.DoubleType(), 0);
        LLVMValueRef loopCondRef = LLVM.BuildFCmp(builder, LLVMRealPredicate.LLVMRealONE, endCondRef, zeroRef, "loopcond");

        // generate the post loop basic block
        LLVMBasicBlockRef endOfLoopBlock = LLVM.GetInsertBlock(builder);
        LLVMBasicBlockRef postLoopBlock = LLVM.AppendBasicBlock(parentBlock, "postloop");

        //create the condition break to evalaute where to go (ie run loop again or break out of loop)
        LLVM.BuildCondBr(builder, loopCondRef, loopBlock, postLoopBlock);

        //reposition the builder
        LLVM.PositionBuilderAtEnd(builder, postLoopBlock);

        //various cleanups are below 

        //update the phivarref with the new values
        LLVM.AddIncoming(phiVarRef, new LLVMValueRef[] { nextVarRef }, new LLVMBasicBlockRef[] { endOfLoopBlock }, 1);

        //either replace the phiVarRef with the old variable in the vars dictionary, or remove it altogether
        if (oldVariablePresent)
        {
            namedValuesLLVM[forLoop.index.name] = oldVariableRef;
        }
        else
        {
            Console.WriteLine("removing phi var ref from named values");
            namedValuesLLVM.Remove(forLoop.index.name);
        }

        valueStack.Push(LLVM.ConstReal(LLVMTypeRef.DoubleType(), 0));

    }

    public static void generatePhiVar(PhiVariable phiVar)
    {
        LLVMValueRef phiVarRef = LLVM.BuildPhi(builder, LLVMTypeRef.DoubleType(), phiVar.name);
        valueStack.Push(phiVarRef);
    }

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
            case ASTNode.NodeType.ForLoop:
                generateForLoop((ForLoop)node);
                break;
            case ASTNode.NodeType.PhiVariable:
                generatePhiVar((PhiVariable)node);
                break;

        }
    }

    public static void generateIR(List<ASTNode> nodes, LLVMBuilderRef _builder, LLVMModuleRef _module, LLVMPassManagerRef _passManager)
    {
        builder = _builder;
        module = _module;
        passManager = _passManager;


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

        // LLVM.RunPassManager(passManager, module);

        Console.WriteLine("LLVM module dump below");
        LLVM.DumpModule(module);
        Console.WriteLine("");
    }

}
