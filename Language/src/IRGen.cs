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

    public static LLVMModuleRef module;

    public static LLVMBuilderRef builder;

    public static LLVMPassManagerRef passManager;

    public static readonly Stack<LLVMValueRef> valueStack = new Stack<LLVMValueRef>();

    public static readonly Stack<LLVMTypeRef> typeStack = new Stack<LLVMTypeRef>();

    public static Dictionary<string, LLVMValueRef> namedValuesLLVM = new Dictionary<string, LLVMValueRef>();

    public static Dictionary<string, AST.VariableDeclaration> namedGlobalsAST = new Dictionary<string, AST.VariableDeclaration>();

    public static LLVMBasicBlockRef mainEntryBlock;
    public static bool mainBuilt = false;
    public static List<AST.Node> nodesToBuild = new List<AST.Node>();

    public static Dictionary<string, LLVMValueRef> namedMutablesLLVM = new Dictionary<string, LLVMValueRef>();

    public static AST.Type LLVMTypeToASTType(LLVMTypeRef type, AST.Node parent)
    {
        switch (type.TypeKind)
        {
            case LLVMTypeKind.LLVMDoubleTypeKind:
                return new AST.Type(new Util.Token(Util.TokenType.Keyword, "double", parent.line, parent.column));
            case LLVMTypeKind.LLVMIntegerTypeKind:
                return new AST.Type(new Util.Token(Util.TokenType.Keyword, "int", parent.line, parent.column));
        }

        throw GenException.FactoryMethod($"An unknown or unsupported type ({type.TypeKind.ToString()}) was used", "You used an undefined or illegal type | Likely a typo", parent, true, type.TypeKind.ToString());
    }




    public static void generateIR(List<AST.Node> nodes, LLVMBuilderRef _builder, LLVMModuleRef _module, LLVMPassManagerRef _passManager)
    {
        builder = _builder;
        module = _module;
        passManager = _passManager;


        foreach (AST.Node node in nodes)
        {
            node.generator.generate();
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
