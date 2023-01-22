using LLVMSharp;

public class IRGen
{
    public static Stack<IRGen> generatorStack = new Stack<IRGen>();

    public static IRGen getInstance()
    {
        return generatorStack.Peek();
    }

    public static IRGen addInstance(LLVMBuilderRef _builder, LLVMModuleRef _module, LLVMPassManagerRef _passManager, /* LLVMContextRef context, */ Parser _parser, string fileName)
    {
        IRGen newGen = new IRGen(_builder, _module, _passManager, /* context, */ _parser, fileName);
        generatorStack.Push(newGen);
        return newGen;
    }

    public static IRGen removeInstance()
    {
        return generatorStack.Pop();
    }

    public IRGen(LLVMBuilderRef _builder, LLVMModuleRef _module, LLVMPassManagerRef _passManager, /* LLVMContextRef context, */ Parser parser, string fileName)
    {
        DebugConsole.WriteAnsi($"[blue]fileName: {fileName}[/]");
        this.fileName = fileName;
        builder = _builder;
        module = _module;
        passManager = _passManager;
        this.parser = parser;
        // this.context = context;
    }

    public Parser parser;

    public string fileName = "";

    public int maxStringIntLength = 64;

    public LLVMContextRef context;

    public LLVMModuleRef module;

    public LLVMBuilderRef builder;

    public LLVMPassManagerRef passManager;

    public Stack<LLVMValueRef> valueStack = new Stack<LLVMValueRef>();
    public Stack<LLVMTypeRef> typeStack = new Stack<LLVMTypeRef>();

    public Dictionary<string, LLVMValueRef> namedValuesLLVM = new Dictionary<string, LLVMValueRef>();
    public Dictionary<string, AST.Struct> namedTypesAST = new Dictionary<string, AST.Struct>();
    public Dictionary<string, LLVMTypeRef> namedTypesLLVM = new Dictionary<string, LLVMTypeRef>();

    // public Stack<Dictionary<string, LLVMTypeRef>> namedTypesLLVMStack = new Stack<Dictionary<string, LLVMTypeRef>>();
    // public Stack<Dictionary<string, AST.Struct>> namedTypesASTStack = new Stack<Dictionary<string, AST.Struct>>();
    public Stack<Dictionary<string, LLVMValueRef>> namedValuesLLVMStack = new Stack<Dictionary<string, LLVMValueRef>>();

    public LLVMValueRef getNamedValueInScope(string name)
    {
        LLVMValueRef val;
        namedValuesLLVMStack.Peek().TryGetValue(name, out val);
        return val;
    }

    public void clearNamedValueScope()
    {
        namedValuesLLVMStack.Pop();
    }

    public void addLayerToNamedValueStack()
    {
        if (namedValuesLLVMStack.Count > 0)
        {
            namedValuesLLVMStack.Push(new Dictionary<string, LLVMValueRef>(namedValuesLLVMStack.Peek()));
        }
        else
        {
            namedValuesLLVMStack.Push(new Dictionary<string, LLVMValueRef>());
        }
    }

    public bool valueExistsInScope(string name)
    {
        return namedValuesLLVMStack.Peek().ContainsKey(name);
    }


    public void addNamedValueInScope(string name, LLVMValueRef value)
    {
        try
        {

            namedValuesLLVMStack.Peek().Add(name, value);
        }
        catch (Exception e)
        {
            DebugConsole.DumpModule(module);
            throw e;
        }
    }

    // public AST.Struct getNamedTypeASTInScope(string name)
    // {
    //     AST.Struct str;
    //     namedTypesASTStack.Pop().TryGetValue(name, out str);
    //     return str;
    // }

    // public LLVMTypeRef getNamedTypeLLVMInScope(string name)
    // {
    //     LLVMTypeRef val;
    //     namedTypesLLVMStack.Pop().TryGetValue(name, out val);
    //     return val;
    // }



    // public void addNamedTypeLLVMInScope(string name, LLVMTypeRef type)
    // {
    //     namedTypesLLVMStack.Pop().Add(name, type);
    // }
    //
    // public void addNamedTypeASTInScope(string name, AST.Struct str)
    // {
    //     namedTypesASTStack.Pop().Add(name, str);
    // }

    public LLVMBasicBlockRef mainEntryBlock;
    public bool mainBuilt = false;
    public List<AST.Node> nodesToBuild = new List<AST.Node>();

    public Stack<AST.Struct> currentStruct = new Stack<AST.Struct>();

    public int getStructFieldIndex(AST.VariableExpression varExpr)
    {
        int index = currentStruct.Pop().getPropertyIndex(varExpr.value);
        return index;
    }

    public int getStructFieldIndex(string fieldName)
    {
        int index = currentStruct.Pop().getPropertyIndex(fieldName);
        return index;
    }

    public TypeInformation LLVMTypeToASTType(LLVMTypeRef type, AST.Node parent)
    {
        switch (type.TypeKind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
                return new ParserTypeInformation("int");
            case LLVMTypeKind.LLVMIntegerTypeKind:
                return new ParserTypeInformation("int");
            case LLVMTypeKind.LLVMPointerTypeKind:
                return new ParserTypeInformation("int");
        }

        throw GenException.FactoryMethod($"An unknown or unsupported type ({type.TypeKind.ToString()}) was used", "You used an undefined or illegal type | Likely a typo", parent, true, type.TypeKind.ToString());
    }

    public LLVMValueRef recursiveDeReference(LLVMValueRef startRef)
    {
        LLVMTypeRef typeRef = LLVM.TypeOf(startRef);

        DebugConsole.Write("typeRef kind: " + typeRef.TypeKind);

        if (typeRef.TypeKind == LLVMTypeKind.LLVMPointerTypeKind)
        {
            LLVMValueRef loadRef = getDereference(startRef);

            return recursiveDeReference(loadRef);
        }
        else
        {
            return startRef;
        }


    }

    public LLVMValueRef getDereference(LLVMValueRef startRef)
    {
        if (startRef.TypeOf().TypeKind != LLVMTypeKind.LLVMPointerTypeKind)
        {
            return startRef;
        }
        LLVMValueRef loadRef = LLVM.BuildLoad(builder, startRef, "loadtmp");
        return loadRef;
    }

    public int compareTypeInheritance(TypeInformation type1, TypeInformation type2)
    {
        return 0;
    }

    public LLVMValueRef getReference(LLVMValueRef startRef)
    {
        return new LLVMValueRef();
    }

    public (string, AST.Prototype, int) getDeclaredFunction(string input, AST.FunctionCall caller)
    {
        string nameToSearch = input;

        if (parser.declaredFuncs.ContainsKey(nameToSearch))
        {
            return (nameToSearch, parser.declaredFuncs[nameToSearch], -1);
        }

        nameToSearch = caller.functionName;
        DebugConsole.Write("new name to search: " + nameToSearch);

        List<AST.Prototype> protosMatchingNameAndArgCount = new List<AST.Prototype>();

        foreach (KeyValuePair<string, AST.Prototype> pair in parser.declaredFuncs)
        {
            if (pair.Key.Contains(nameToSearch) && pair.Value.arguments.Count == caller.args.Count)
            {
                protosMatchingNameAndArgCount.Add(pair.Value);
            }
        }

        if (protosMatchingNameAndArgCount.Count == 0)
        {
            if (caller.parent.nodeType == AST.Node.NodeType.VariableExpression)
            {
                //cast caller.parent to an AST.VariablExpression
                AST.VariableExpression varExpr = (AST.VariableExpression)caller.parent;
                if (varExpr.type.isTrait)
                {
                    DebugConsole.Write("in the weird trait function bs");
                    AST.StructTrait trait = parser.declaredStructTraits[varExpr.type.value];
                    List<AST.Prototype> traitProtos = new List<AST.Prototype>();
                    string newNameIHATEMYSELF = nameToSearch.Remove(0, nameToSearch.IndexOf("_") + 1);
                    DebugConsole.Write("caller func name: " + newNameIHATEMYSELF);
                    for (int i = 0; i < trait.protos.Count; i++)
                    {
                        DebugConsole.Write(trait.protos[i].name);
                        if (trait.protos[i].name.Contains(newNameIHATEMYSELF))
                        {
                            traitProtos.Add(trait.protos[i]);
                        }
                    }
                    if (traitProtos.Count == 0)
                    {
                        throw GenException.FactoryMethod($"No function with the name {nameToSearch} was found (WITHIN TRAIT PROTOS)", "You tried to call a function that doesn't exist - possible typo in the function call or mismatch arguments", caller, true, nameToSearch);
                    }
                    (string name, AST.Prototype actualProto) = checkProtoArgTypes(nameToSearch, caller, traitProtos);
                    int idxInTraitVal = trait.protos.IndexOf(actualProto);
                    return (name, actualProto, idxInTraitVal);
                }
            }
            throw GenException.FactoryMethod($"No function with the name {nameToSearch} was found (AKSJDKAJSD)", "You tried to call a function that doesn't exist - possible typo in the function call or mismatch arguments", caller, true, nameToSearch);
        }

        (string retName, AST.Prototype retProto) = checkProtoArgTypes(nameToSearch, caller, protosMatchingNameAndArgCount);
        return (retName, retProto, -1);

    }

    public (string, AST.Prototype) checkProtoArgTypes(string nameToSearch, AST.FunctionCall caller, List<AST.Prototype> listToCheck)
    {
        foreach (AST.Prototype proto in listToCheck)
        {
            DebugConsole.Write(proto.name);
            int idx = 0;
            foreach (AST.Type argType in proto.arguments.Values)
            {
                DebugConsole.Write("arg type: " + argType.value);
                DebugConsole.Write("caller arg type: " + caller.args[idx].type.value);
                if (argType.value == caller.args[idx].type.value || argType.value == "void")
                {
                    idx++;
                    continue;
                }
                else if (argType.isTrait && caller.args[idx].type.isStruct)
                {
                    if (parser.declaredStructs.ContainsKey(caller.args[idx].type.value) || parser.declaredStructTraits.ContainsKey(caller.args[idx].type.value))
                    {
                        DebugConsole.Write("detected arg that is a struct - caller args[idx].value is: " + caller.args[idx].type.value);
                        AST.Struct str = parser.declaredStructs[caller.args[idx].type.value];
                        if (str.implementedTraits.Contains(parser.declaredStructTraits[argType.value]))
                        {
                            idx++;
                            continue;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }


                idx++;
            }
            if (idx == proto.arguments.Count)
            {
                return (proto.name, proto);
            }
        }
        throw GenException.FactoryMethod($"No function with the name {nameToSearch} was found (THIS IS IN THAT LOOPY FUNC)", "You tried to call a function that doesn't exist", caller, true, nameToSearch);
    }

    public void generateIR(List<AST.Node> nodes, Spectre.Console.ProgressTask task)
    {
        task.MaxValue = nodes.Count;

        foreach (AST.Node node in nodes)
        {
            DebugConsole.Write("generating node of type " + node.nodeType);
            node.generator.generate();
            DebugConsole.Write("successfully evaluated node of type " + node.nodeType);
            task.Increment(1);
        }

        DebugConsole.WriteAnsi("[red]module verify below[/]");
        DebugConsole.VerifyModule(module);
        DebugConsole.WriteAnsi("[red]module verify end[/]");

        DebugConsole.WriteAnsi("[blue]pre optimizations LLVM IR below [/]");
        DebugConsole.DumpModule(module);
        DebugConsole.Write("");
    }

    public void generateIR(List<AST.Node> nodes)
    {
        DebugConsole.WriteAnsi("[red]genning file named " + fileName + "[/]");
        foreach (AST.Node node in nodes)
        {
            DebugConsole.Write("generating node of type " + node.nodeType);
            node.generator.generate();
            DebugConsole.Write("successfully evaluated node of type " + node.nodeType);
        }

        DebugConsole.WriteAnsi("[red]module verify below[/]");
        DebugConsole.VerifyModule(module);
        DebugConsole.WriteAnsi("[red]module verify end[/]");

        DebugConsole.WriteAnsi("[blue]pre optimizations LLVM IR below [/]");
        DebugConsole.DumpModule(module);
        DebugConsole.Write("");
    }

    public void optimizeIR(Spectre.Console.ProgressTask task)
    {
        LLVM.RunPassManager(passManager, module);

        DebugConsole.WriteAnsi("[blue]post optimizations LLVM IR below [/]");
        DebugConsole.DumpModule(module);
        DebugConsole.Write("");
        task.StopTask();
    }

    public void optimizeIR()
    {
        LLVM.RunPassManager(passManager, module);

        DebugConsole.WriteAnsi("[blue]post optimizations LLVM IR below [/]");
        DebugConsole.DumpModule(module);
        DebugConsole.Write("");
    }

    public void test()
    {
    }
}
