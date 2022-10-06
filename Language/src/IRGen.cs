using LLVMSharp;

using Spectre.Console;

public class IRGen
{
    public static Stack<IRGen> generatorStack = new Stack<IRGen>();

    public static IRGen getInstance()
    {
        return generatorStack.Peek();
    }

    public static IRGen addInstance(LLVMBuilderRef _builder, LLVMModuleRef _module, LLVMPassManagerRef _passManager, LLVMContextRef _context)
    {
        IRGen newGen = new IRGen(_builder, _module, _passManager, _context);
        generatorStack.Push(newGen);
        return newGen;
    }

    public static IRGen removeInstance()
    {
        return generatorStack.Pop();
    }

    public IRGen(LLVMBuilderRef _builder, LLVMModuleRef _module, LLVMPassManagerRef _passManager, LLVMContextRef _context)
    {
        builder = _builder;
        module = _module;
        passManager = _passManager;
        context = _context;
    }

    public int maxStringIntLength = 64;

    public LLVMContextRef context;

    public LLVMModuleRef module;

    public LLVMBuilderRef builder;

    public LLVMPassManagerRef passManager;

    public readonly Stack<LLVMValueRef> valueStack = new Stack<LLVMValueRef>();
    public readonly Stack<LLVMTypeRef> typeStack = new Stack<LLVMTypeRef>();

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
        namedValuesLLVMStack.Peek().Add(name, value);
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
        int index = currentStruct.Pop().getPropIndex(varExpr.value);

        return index;
    }

    public int getStructFieldIndex(string fieldName)
    {
        int index = currentStruct.Pop().getPropIndex(fieldName);

        return index;
    }

    public AST.Type LLVMTypeToASTType(LLVMTypeRef type, AST.Node parent)
    {
        switch (type.TypeKind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
                return new AST.Type(new Util.Token(Util.TokenType.Keyword, "double", parent.line, parent.column));
            case LLVMTypeKind.LLVMIntegerTypeKind:
                return new AST.Type(new Util.Token(Util.TokenType.Keyword, "int", parent.line, parent.column));
            case LLVMTypeKind.LLVMPointerTypeKind:
                return new AST.Type(new Util.Token(Util.TokenType.Keyword, "int", parent.line, parent.column));
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

    public LLVMValueRef getReference(LLVMValueRef startRef)
    {
        return new LLVMValueRef();
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
}
