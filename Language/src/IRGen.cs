using LLVMSharp;

using Spectre.Console;

public static class IRGen
{
    public static int maxStringIntLength = 64;

    public static LLVMContextRef context;

    public static LLVMModuleRef module;

    public static LLVMBuilderRef builder;

    public static LLVMPassManagerRef passManager;

    public static readonly Stack<LLVMValueRef> valueStack = new Stack<LLVMValueRef>();
    public static readonly Stack<LLVMTypeRef> typeStack = new Stack<LLVMTypeRef>();


    public static Dictionary<string, LLVMValueRef> namedValuesLLVM = new Dictionary<string, LLVMValueRef>();
    public static Dictionary<string, AST.Struct> namedTypesAST = new Dictionary<string, AST.Struct>();
    public static Dictionary<string, LLVMTypeRef> namedTypesLLVM = new Dictionary<string, LLVMTypeRef>();


    public static Stack<Dictionary<string, LLVMTypeRef>> namedTypesLLVMStack = new Stack<Dictionary<string, LLVMTypeRef>>();
    public static Stack<Dictionary<string, AST.Struct>> namedTypesASTStack = new Stack<Dictionary<string, AST.Struct>>();
    public static Stack<Dictionary<string, LLVMValueRef>> namedValuesLLVMStack = new Stack<Dictionary<string, LLVMValueRef>>();

    public static LLVMValueRef getNamedValueInScope(string name)
    {
        LLVMValueRef val;
        namedValuesLLVMStack.Pop().TryGetValue(name, out val);
        return val;
    }

    public static AST.Struct getNamedTypeASTInScope(string name)
    {
        AST.Struct str;
        namedTypesASTStack.Pop().TryGetValue(name, out str);
        return str;
    }

    public static LLVMTypeRef getNamedTypeLLVMInScope(string name)
    {
        LLVMTypeRef val;
        namedTypesLLVMStack.Pop().TryGetValue(name, out val);
        return val;
    }

    public static void addNamedValueInScope(string name, LLVMValueRef value)
    {
        namedValuesLLVMStack.Pop().Add(name, value);
    }

    public static void addNamedTypeLLVMInScope(string name, LLVMTypeRef type)
    {
        namedTypesLLVMStack.Pop().Add(name, type);
    }

    public static void addNamedTypeASTInScope(string name, AST.Struct str)
    {
        namedTypesASTStack.Pop().Add(name, str);
    }

    public static LLVMBasicBlockRef mainEntryBlock;
    public static bool mainBuilt = false;
    public static List<AST.Node> nodesToBuild = new List<AST.Node>();

    public static Stack<AST.Struct> currentStruct = new Stack<AST.Struct>();

    public static int getStructFieldIndex(AST.VariableExpression varExpr)
    {
        int index = currentStruct.Pop().getPropIndex(varExpr.value);

        return index;
    }

    public static int getStructFieldIndex(string fieldName)
    {
        int index = currentStruct.Pop().getPropIndex(fieldName);

        return index;
    }

    public static AST.Type LLVMTypeToASTType(LLVMTypeRef type, AST.Node parent)
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

    public static LLVMValueRef recursiveDeReference(LLVMValueRef startRef)
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

    public static LLVMValueRef getDereference(LLVMValueRef startRef)
    {
        if (startRef.TypeOf().TypeKind != LLVMTypeKind.LLVMPointerTypeKind)
        {
            return startRef;
        }
        LLVMValueRef loadRef = LLVM.BuildLoad(builder, startRef, "loadtmp");
        return loadRef;
    }

    public static LLVMValueRef getReference(LLVMValueRef startRef)
    {
        return new LLVMValueRef();
    }


    public static void initialize(LLVMBuilderRef _builder, LLVMModuleRef _module, LLVMPassManagerRef _passManager, LLVMContextRef _context)
    {
        builder = _builder;
        module = _module;
        passManager = _passManager;
        context = _context;
    }

    public static void generateIR(List<AST.Node> nodes, Spectre.Console.ProgressTask task)
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

    public static void generateIR(List<AST.Node> nodes)
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

    public static void optimizeIR(Spectre.Console.ProgressTask task)
    {
        LLVM.RunPassManager(passManager, module);

        DebugConsole.WriteAnsi("[blue]post optimizations LLVM IR below [/]");
        DebugConsole.DumpModule(module);
        DebugConsole.Write("");
        task.StopTask();
    }

    public static void optimizeIR()
    {
        LLVM.RunPassManager(passManager, module);

        DebugConsole.WriteAnsi("[blue]post optimizations LLVM IR below [/]");
        DebugConsole.DumpModule(module);
        DebugConsole.Write("");
    }
}
