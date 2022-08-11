using LLVMSharp;

using Spectre.Console;

/*below is gep generation (prob useless)
 //below zero next to ulong is the index of the element you want to grab a pointer to
        LLVMValueRef[] arrIndices = { LLVM.ConstInt(LLVM.Int64Type(), (ulong)0, false) };
        LLVMValueRef gepRef = LLVM.BuildInBoundsGEP(builder, globalRef, arrIndices, varExp.varName);
        valueStack.Push(gepRef);
 */

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

    public static Dictionary<string, AST.VariableDeclaration> namedGlobalsAST = new Dictionary<string, AST.VariableDeclaration>();

    public static Dictionary<string, AST.Struct> namedTypesAST = new Dictionary<string, AST.Struct>();

    public static Dictionary<string, LLVMTypeRef> namedTypesLLVM = new Dictionary<string, LLVMTypeRef>();

    public static LLVMBasicBlockRef mainEntryBlock;

    public static bool mainBuilt = false;
    public static List<AST.Node> nodesToBuild = new List<AST.Node>();

    public static Dictionary<string, LLVMValueRef> namedMutablesLLVM = new Dictionary<string, LLVMValueRef>();

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
            Console.WriteLine("generating node of type " + node.nodeType);
            node.generator.generate();
            Console.WriteLine("successfully evaluated node of type " + node.nodeType);
            task.Increment(1);

            // foreach (ASTNode child in node.children)
            // {
            //     evaluateNode(child);
            // }
            // Console.WriteLine("stack dump");
            // LLVM.DumpValue(valueStack.Peek());
        }

        LLVM.VerifyModule(module, LLVMVerifierFailureAction.LLVMPrintMessageAction, out string verifyMessage);

        AnsiConsole.MarkupLine("[blue]pre optimizations LLVM IR below [/]");
        LLVM.DumpModule(module);
        Console.WriteLine("");
    }

    public static void optimizeIR(Spectre.Console.ProgressTask task)
    {
        LLVM.RunPassManager(passManager, module);

        AnsiConsole.MarkupLine("[blue]post optimizations LLVM IR below [/]");
        LLVM.DumpModule(module);
        Console.WriteLine("");
        task.StopTask();
    }

}
