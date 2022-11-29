//TODO: double check the previous node stuff for binexpr
using System.Text;
using static System.Text.Json.JsonSerializer;

public class Parser
{
    public static List<AST.Node.NodeType> exportTypes = new List<AST.Node.NodeType>() { AST.Node.NodeType.Prototype, AST.Node.NodeType.Struct };
    //NOTE: below is a stack containing instances of parser (used for multi file)
    public static Stack<Parser> parserStack = new Stack<Parser>();

    public static Parser addInstance(List<Util.Token> tokenList, string fileName, string path, Spectre.Console.ProgressTask task = null)
    {
        Parser newParser = new Parser(tokenList, fileName, path, task);
        parserStack.Push(newParser);
        return newParser;
    }

    public static Parser removeInstance()
    {
        if (parserStack.Count > 0)
            return parserStack.Pop();
        else
            return null;
    }

    public static Parser getInstance()
    {
        if (parserStack.Count > 0)
            return parserStack.Peek();
        else
            return null;
    }

    public string fileName = "";
    public string filePath = "";

    public bool singleLineComment = false;
    public bool multiLineComment = false;

    public int delimLevel = 0;
    public int prevLine = 0;
    public int prevColumn = 0;

    public Spectre.Console.ProgressTask progressTask;

    public List<AST.Node> nodes = new List<AST.Node>();
    public List<Util.Token> tokenList;

    public List<AST.VariableDeclaration> globalVarAss = new List<AST.VariableDeclaration>();

    public Parser parentParser;

    public Util.TokenType[] binaryExpectedTokens = { Util.TokenType.Int, Util.TokenType.Keyword };
    public Util.TokenType[] delimiterExpectedTokens = { Util.TokenType.Keyword };
    public AST.Node.NodeType[] binaryExpectedNodes = { AST.Node.NodeType.NumberExpression, AST.Node.NodeType.BinaryExpression, AST.Node.NodeType.VariableExpression, AST.Node.NodeType.PhiVariable };

    // public Dictionary<string, AST.Function> declaredFunctionDict = new Dictionary<string, AST.Function>();
    public Dictionary<string, AST.VariableDeclaration> declaredGlobalsDict = new Dictionary<string, AST.VariableDeclaration>();
    //HACK: might want to do this differently
    //NOTE: modified name
    public Dictionary<string, AST.Prototype> declaredFuncs = new Dictionary<string, AST.Prototype>();
    public Dictionary<string, AST.Struct> declaredStructs = new Dictionary<string, AST.Struct>();

    //NOTE: below can be used to add user defined types (structs/classes)
    public List<string> typeList = new List<string>() { "double", "float", "string" };

    //NOTE: below are all for the while loop func
    public int finalTokenNum = 0;
    public int currentTokenNum = -1;
    public bool isFinishedParsing = false;
    public Stack<AST.Node> delimParentStack = new Stack<AST.Node>();

    public AST.Node lastMajorParentNode = new AST.Empty();

    public AST.Node parent;

    public AST.Node previousNode;

    public string[] binaryMathOps = { "+", "-", "*", "/" };

    public Stack<Dictionary<string, TypeInformation>> variablesTypeStack = new Stack<Dictionary<string, TypeInformation>>();

    public void writeAST()
    {
        ASTFile ast = new ASTFile(this);
        ast.write();
    }

    public TypeInformation getNamedValueInScopeType(string name)
    {
        if (variablesTypeStack.Peek().ContainsKey(name))
        {
            return variablesTypeStack.Peek()[name];
        }
        throw new ParserException("An unknown variable was referenced");
    }

    public TypeInformation getNamedValueInScope(string name, AST.Node caller)
    {
        if (variablesTypeStack.Peek().ContainsKey(name))
        {
            return variablesTypeStack.Peek()[name];
        }
        throw ParserException.FactoryMethod("An unknown variable was referenced", "Remove the reference", caller, true);
    }

    public void clearNamedASTStack()
    {
        variablesTypeStack.Pop();
    }

    public void addLayerToNamedASTStack()
    {
        DebugConsole.WriteAnsi("[yellow]adding layer to stack[/]");
        if (variablesTypeStack.Count > 0)
        {
            variablesTypeStack.Push(new Dictionary<string, TypeInformation>(variablesTypeStack.Peek()));
        }
        else
        {
            variablesTypeStack.Push(new Dictionary<string, TypeInformation>());
        }
    }

    public void removeLayerFromASTStack()
    {
        if (variablesTypeStack.Count > 0)
        {
            DebugConsole.WriteAnsi("[yellow]removing layer[/]");
            variablesTypeStack.Pop();
        }
    }

    public bool valueExistsInScopeAST(string name)
    {
        return variablesTypeStack.Peek().ContainsKey(name);
    }
    public void addNamedValueInScope(string name, TypeInformation type, AST.Node caller)
    {
        //TODO: handle variable declared that already exists
        DebugConsole.Write("add named to scope called with name " + name);
        if (variablesTypeStack.Peek().ContainsKey(name))
        {
            throw ParserException.FactoryMethod($"Illegal re-definition of variable named {name} with type {type.value}", "Remove the re-definition", caller, true, name);
        }
        variablesTypeStack.Peek().Add(name, type);
    }

    public void addNamedValueInScope(string name, TypeInformation type)
    {
        //TODO: handle variable declared that already exists
        DebugConsole.Write("add named to scope called with name " + name);
        if (variablesTypeStack.Peek().ContainsKey(name))
        {
            throw ParserException.FactoryMethod($"Illegal re-definition of variable named {name} with type {type.value}", "Remove the re-definition", name);
        }
        variablesTypeStack.Peek().Add(name, type);
    }

    public static bool isAnArrayRef(Util.Token token)
    {
        return (token.value.Contains("[") && token.value.IndexOf("]") > token.value.IndexOf("["));
    }

    public bool isType(Util.Token token)
    {
        if (typeList.Contains(token.value))
        {
            return true;
        }
        (bool isInt, int bits) = checkInt(token.value);
        return isInt;
    }

    public bool isType(string value)
    {
        if (typeList.Contains(value))
        {
            return true;
        }
        (bool isInt, int bits) = checkInt(value);
        return isInt;
    }

    public (bool, int) checkInt(string value)
    {
        if (value.EndsWith("*"))
        {
            value = value.Remove(value.Length - 1);
        }
        if (value.StartsWith("int"))
        {
            string strBits = value.Remove(0, 3);

            if (strBits == "")
            {
                return (true, 32);
            }

            if (int.TryParse(strBits, out int bits))
            {
                return (true, bits);
            }
        }
        else if (value.StartsWith("uint"))
        {
            string strBits = value.Remove(0, 4);

            if (strBits == "")
            {
                return (true, 32);
            }

            if (int.TryParse(strBits, out int bits))
            {
                return (true, bits);
            }
        }
        else if (value.StartsWith("i"))
        {
            string strBits = value.Remove(0, 1);

            if (int.TryParse(strBits, out int bits))
            {
                return (true, bits);
            }
        }
        else if (value.StartsWith("ui"))
        {
            string strBits = value.Remove(0, 2);

            if (int.TryParse(strBits, out int bits))
            {
                return (true, bits);
            }
        }
        return (false, 0);
    }


    public static void checkNode(AST.Node node, AST.Node.NodeType[] expectedTypes)
    {
        if (node == null)
        {
            // throw new ParserException();
        }

        foreach (AST.Node.NodeType expectedNodeType in expectedTypes)
        {
            if (node.nodeType != expectedNodeType && expectedNodeType == expectedTypes.Last())
            {
                throw new ParserException($"expected type {string.Join(", ", expectedTypes)} but got {node.nodeType}", node);
            }
            else if (node.nodeType == expectedNodeType)
            {
                break;
            }
        }
    }

    public void checkNode(AST.Node node, AST.Node.NodeType[] expectedTypes, ParserException except)
    {
        if (node == null)
        {
            throw new ParserException($"expected a node at but got null", prevLine, prevColumn);
        }

        foreach (AST.Node.NodeType expectedNodeType in expectedTypes)
        {
            if (node.nodeType != expectedNodeType && expectedNodeType == expectedTypes.Last())
            {
                throw except;
            }
            else if (node.nodeType == expectedNodeType)
            {
                break;
            }
        }
    }

    public void checkToken(Util.Token token, List<Util.TokenType> expectedTypes)
    {
        foreach (Util.TokenType expectedTokenType in expectedTypes)
        {
            if (token.type != expectedTokenType && expectedTokenType == expectedTypes.Last())
            {
                throw new ParserException($"expected token of type {string.Join(", ", expectedTypes)} but got {token.type}", token);
            }
            else if (token.type == expectedTokenType)
            {
                break;
            }
        }

    }


    public void checkToken(Util.Token token, Util.TokenType expectedType)
    {
        if (token.type != expectedType)
        {
            throw new ParserException($"expected token of type {expectedType} but got {token.type}", token);
        }
    }

    public string printBinary(AST.BinaryExpression bin)
    {
        StringBuilder stringBuilder = new StringBuilder();
        // stringBuilder.Append($"{bin.nodeType} op: {bin.operatorType} lhs type: {bin.leftHand.nodeType} rhs type: {bin?.rightHand?.nodeType} binop children below:");
        // stringBuilder.Append(printASTRet(bin?.children));

        return "";
    }

    public string printFunc(AST.Function func)
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (func.prototype.arguments.Count() > 0)
        {
            stringBuilder.Append($"first arg type: {printType((ParserTypeInformation)func.prototype.arguments.ElementAt(0).Value)}");
        }
        stringBuilder.Append($"{func.nodeType} name: {func.prototype.name} args: {Serialize(func.prototype.arguments.ToList())} body start: ");
        stringBuilder.Append(printASTRet(func.body));

        stringBuilder.Append("function body end");

        return stringBuilder.ToString();
    }

    public string printFuncCall(AST.FunctionCall funcCall)
    {
        return $"{funcCall.nodeType} with name of {funcCall.functionName} and args of {String.Join(", ", funcCall.args)}";
    }

    public string printVarAss(AST.VariableAssignment varAss)
    {
        return $"{varAss?.nodeType} with name of {varAss?.varExpr?.value} and children of [{printASTRet(varAss?.children)}]";
    }

    public string printProto(AST.Prototype proto)
    {
        return $"{proto.nodeType} with name of {proto.name}";
    }

    public string printIfStat(AST.IfStatement ifStat)
    {
        return $"if statement with expression of {printBinary(ifStat.declaration.expression)} and body of ( {printASTRet(ifStat.thenBody)} ) body end | else statement: {printElseStat(ifStat.elseStat)}";
    }

    public string printElseStat(AST.ElseStatement elseStat)
    {
        return $"else statement with body of ( {printASTRet(elseStat.elseBody)} )";
    }

    public string printForLoop(AST.ForLoop forLoop)
    {
        // return $"For loop with iteration object of {forLoop.iterationObject} and index obj of {printPhiVar(forLoop.index)} complexity of {forLoop.complex} and body of ( {printASTRet(forLoop.body)} ) body end";
        return $"";
    }

    public string printType(TypeInformation type)
    {
        return $"type with name of {type.value} and is array of {type.isArray}";
    }

    public string printASTRet(List<AST.Node> nodesPrint)
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach (AST.Node node in nodesPrint)
        {
            switch (node.nodeType)
            {
                case AST.Node.NodeType.BinaryExpression:
                    AST.BinaryExpression bin = (AST.BinaryExpression)node;
                    stringBuilder.Append(printBinary(bin));
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.Function:
                    AST.Function func = (AST.Function)node;
                    stringBuilder.Append(printFunc(func));
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.FunctionCall:
                    AST.FunctionCall funcCall = (AST.FunctionCall)node;
                    stringBuilder.Append(printFuncCall(funcCall));
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.VariableAssignment:
                    AST.VariableAssignment varAss = (AST.VariableAssignment)node;
                    stringBuilder.Append(printVarAss(varAss));
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.Prototype:
                    AST.Prototype proto = (AST.Prototype)node;
                    stringBuilder.Append(printProto(proto));
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.NumberExpression:
                    AST.NumberExpression numExp = (AST.NumberExpression)node;
                    stringBuilder.Append($"{numExp.nodeType} of value {numExp.value}");
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.IfStatement:
                    AST.IfStatement ifStat = (AST.IfStatement)node;
                    stringBuilder.Append(printIfStat(ifStat));
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.ForLoop:
                    AST.ForLoop forLoop = (AST.ForLoop)node;
                    stringBuilder.Append(printForLoop(forLoop));
                    stringBuilder.Append("\n");
                    break;
                default:
                    stringBuilder.Append(node.nodeType);
                    stringBuilder.Append("\n");
                    break;
            }

        }
        return stringBuilder.ToString();
    }

    public void printAST(List<AST.Node> nodesPrint)
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach (AST.Node node in nodesPrint)
        {
            switch (node.nodeType)
            {
                case AST.Node.NodeType.BinaryExpression:
                    AST.BinaryExpression bin = (AST.BinaryExpression)node;
                    stringBuilder.Append(printBinary(bin));
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.Function:
                    AST.Function func = (AST.Function)node;
                    stringBuilder.Append(printFunc(func));
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.FunctionCall:
                    AST.FunctionCall funcCall = (AST.FunctionCall)node;
                    stringBuilder.Append(printFuncCall(funcCall));
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.VariableAssignment:
                    AST.VariableAssignment varAss = (AST.VariableAssignment)node;
                    stringBuilder.Append(printVarAss(varAss));
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.Prototype:
                    AST.Prototype proto = (AST.Prototype)node;
                    stringBuilder.Append(printProto(proto));
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.IfStatement:
                    AST.IfStatement ifStat = (AST.IfStatement)node;
                    stringBuilder.Append(printIfStat(ifStat));
                    stringBuilder.Append("\n");
                    break;
                case AST.Node.NodeType.ForLoop:
                    AST.ForLoop forLoop = (AST.ForLoop)node;
                    stringBuilder.Append(printForLoop(forLoop));
                    stringBuilder.Append("\n");
                    break;
                default:
                    stringBuilder.Append(node.nodeType);
                    stringBuilder.Append("\n");
                    break;
            }


        }

        DebugConsole.Write(stringBuilder);
    }

    public List<Util.Token> getTokensUntil(int startIndex, Util.TokenType stopType)
    {
        List<Util.Token> ret = new List<Util.Token>();
        Util.Token currentToken = tokenList[startIndex];
        int currentIndex = startIndex;

        while (currentToken.type != stopType)
        {
            ret.Add(currentToken);

            currentToken = tokenList[currentIndex + 1];
            currentIndex++;
        }
        return ret;
    }
    public Util.Token nextNonSpace()
    {
        int currentIndex = currentTokenNum + 1;
        Util.Token currentTok = tokenList[currentIndex];

        while (currentTok.type == Util.TokenType.Space)
        {
            currentIndex++;
            currentTok = tokenList[currentIndex];
        }

        return currentTok;
    }

    public (Util.Token, int) nextNonSpace(int startIndex)
    {
        int currentIndex = startIndex + 1;
        Util.Token currentTok = tokenList[currentIndex];

        while (currentTok.type == Util.TokenType.Space)
        {
            currentIndex++;
            currentTok = tokenList[currentIndex];
        }

        return (currentTok, currentIndex);
    }

    public Util.Token nextNonSpecial(int startIndex)
    {
        int currentIndex = startIndex + 1;
        Util.Token currentTok = tokenList[currentIndex];

        while (currentTok.type == Util.TokenType.Space || currentTok.type == Util.TokenType.Special)
        {
            currentIndex++;
            currentTok = tokenList[currentIndex];
        }

        return currentTok;
    }

    public (AST.Node, int) parseKeyword(Util.Token token, int tokenIndex, AST.Node parent, int delimLevel = 0)
    {
        List<dynamic> ret = new List<dynamic>();
        (Util.Token nextToken, int nextTokenIndex) = nextNonSpace(tokenIndex);

        if (token.value == Config.settings.variable.declaration.keyword.mutable)
        {
            AST.VariableDeclaration varDec = new AST.VariableDeclaration(token, true, parent);
            return (varDec, delimLevel);
            // return new List<dynamic>() { varDec, delimLevel };
        }
        else if (token.value == Config.settings.variable.declaration.keyword.constant)
        {
            AST.VariableDeclaration constDec = new AST.VariableDeclaration(token, false, parent);
            return (constDec, delimLevel);
            // return new List<dynamic>() { constDec, delimLevel };
        }
        else if (Config.settings.general.nulls.enabled && token.value == Config.settings.general.nulls.keyword)
        {
            AST.NullExpression nullExpr = new AST.NullExpression(token, parent);
            return (parent, delimLevel);
        }
        else if (token.value == Config.settings.structs.declaration.keyword)
        {
            AST.Struct str = new AST.Struct(token, parent);
            return (str, delimLevel);
        }
        else if (token.value == Config.settings.function.ret.keyword)
        {
            // if (parent?.nodeType == AST.Node.NodeType.Function)
            // {
            AST.Return retNode = new AST.Return(token, parent);
            return (retNode, delimLevel);
            // return new List<dynamic>() { retNode, delimLevel };
            // }
            // else
            // {
            //     throw ParserException.FactoryMethod($"Illegal usage of function return keyword ({Config.settings.function.ret.keyword})", "Delete the keyword | Fix a typo", token, typoSuspected: true);
            // }
        }
        else if (token.value == Config.settings.general.import.keyword)
        {
            AST.ImportStatement importStatement = new AST.ImportStatement(token);
            DebugConsole.WriteAnsi("[red]returning import stat as parent[/]");
            return (importStatement, delimLevel);
            // return new List<dynamic>() { importStatement, delimLevel };
        }
        else if (token.value == Config.settings.general.import.ignore.keyword)
        {
            //TODO: add code here to implement "private"
            //NOTE: I can do this by throwing errors if a private thing is called outside of its file? I will have to store the origin files of stuff
        }
        else if (token.value == "if")
        {
            AST.IfStatementDeclaration ifStat = new AST.IfStatementDeclaration(token, parent);
            return (ifStat, delimLevel);
        }
        else if (token.value == "else")
        {
            AST.IfStatement ifParent = (AST.IfStatement)parent.children.Last();
            return (ifParent.elseStat, delimLevel);
        }
        else if (token.value == "for")
        {
            AST.ForLoop forLoop = new AST.ForLoop(token, parent);
            return (forLoop, delimLevel);
        }
        else if (token.value == "while")
        {
            AST.WhileLoop whileLoop = new AST.WhileLoop(token, parent);
            return (whileLoop, delimLevel);
        }
        else if (token.value == Config.settings.function.declaration.externKeyword)
        {
            AST.ExternStatement externStat = new AST.ExternStatement(token, parent);
            return (externStat, delimLevel);
        }
        else if (Config.settings.function.declaration.marker.word && token.value == Config.settings.function.declaration.marker.value)
        {
            AST.Prototype proto = new AST.Prototype(token, parent);
            return (proto, delimLevel);
        }
        else if (!Config.settings.function.declaration.marker.word)
        {
            if (token.value[0].ToString() == Config.settings.function.declaration.marker.value)
            {
                if (parent?.nodeType == AST.Node.NodeType.Prototype)
                {
                    parent?.addChild(token);
                    return (parent, delimLevel);
                }
                else
                {
                    AST.Prototype proto = new AST.Prototype(token, parent);
                    return (proto, delimLevel);
                }
            }
            else if (nextToken.value[0].ToString() == Config.settings.function.declaration.marker.value && Config.settings.function.declaration.returnTypeLocation == ConfigModel.ReturnTypeLocation.Start)
            {
                AST.Prototype proto = new AST.Prototype(token, parent, startWithRet: true);
                return (proto, delimLevel);
            }
        }
        if (nextToken.value == Config.settings.function.calling.args.delimeters[0] || nextToken.value == "(")
        {
            DebugConsole.WriteAnsi("[purple]making new func call[/]");
            AST.FunctionCall funcCall = new AST.FunctionCall(token, null, parent);
            return (funcCall, delimLevel);
        }



        // else if (parent?.nodeType == AST.Node.NodeType.ForLoop)
        // {
        //     AST.ForLoop forLoop = (AST.ForLoop)parent;
        //     if (!forLoop.isBody)
        //     {
        //         parent.addChild(token);
        //         return new List<dynamic>() { parent, delimLevel };
        //     }
        // }

        switch (parent?.nodeType)
        {
            case AST.Node.NodeType.ForLoop:
                break;
            case AST.Node.NodeType.Prototype:
                AST.Prototype proto = (AST.Prototype)parent;
                proto.addChild(token);
                return (parent, delimLevel);
            case AST.Node.NodeType.VariableAssignment:
                // AST.VariableAssignment varAss = (AST.VariableAssignment)parent;
                break;
            case AST.Node.NodeType.ImportStatement:
                parent.addChild(token);
                DebugConsole.Write("ASDKJASKDj");
                DebugConsole.WriteAnsi($"[green]returning parent of {parent.nodeType}[/]");
                return (parent, delimLevel);
        }



        //below handles no-keyword but with default value variable assignments
        if (!Config.settings.variable.declaration.keyword.forced)
        {
            (Util.Token twoToks, int twoToksIdx) = nextNonSpace(nextTokenIndex);
            if (twoToks.value == "=" || twoToks.value == Config.settings.variable.declaration.keyword.mutable)
            {
                DebugConsole.WriteAnsi("[red]detected no keyword variable dec with equals[/]");
                AST.VariableDeclaration varDec = new AST.VariableDeclaration(token, parent);
                return (varDec, delimLevel);
            }
        }

        //NOTE: this needs to be below the other type or else inital value decs wont work
        //Below handles variable declarations with no initial value and no keyword - it type checks which is slower but necessary
        if (!Config.settings.variable.declaration.keyword.forced)
        {
            if (isType(token))
            {
                DebugConsole.WriteAnsi("[red]detected no keyword variable dec WITHOUT equals[/]");
                return (new AST.VariableDeclaration(token, parent), delimLevel);
            }
        }

        switch (parent?.nodeType)
        {
            case AST.Node.NodeType.VariableDeclaration:
                parent?.addChild(token);
                return (parent, delimLevel);
            case AST.Node.NodeType.Struct:
                parent.addChild(token);
                return (parent, delimLevel);
        }


        //below can handle the nested variable expressions
        //TODO: replace this in favor of special char handling
        AST.VariableExpression varExpr = new AST.VariableExpression(token, parent);
        if (parent.nodeType != AST.Node.NodeType.VariableExpression)
        {
            DebugConsole.Write("returning var expr");
            return (varExpr, delimLevel);
        }
        return (parent, delimLevel);
    }

    public (AST.Node parent, int delimLevel) parseDelim(Util.Token token, int tokenIndex, AST.Node parent, int delimLevel = 0)
    {
        if (token.type == Util.TokenType.DelimiterOpen)
        {
            // if (token.value == "[" && parent?.nodeType != AST.Node.NodeType.VariableDeclaration)
            // {
            //     parent = new AST.IndexReference(token, parent);
            //     delimParentStack.Push(parent);
            //     delimLevel++;
            //     return (parent, delimLevel);
            // }
            bool addLayer = true;
            switch (parent?.nodeType)
            {
                case AST.Node.NodeType.VariableDeclaration:
                    //TODO: replace this with the config delimiter
                    if (token.value == "{")
                    {
                        AST.ArrayExpression arrExpr = new AST.ArrayExpression(token, parent); delimParentStack.Push(arrExpr);
                        return (arrExpr, delimLevel + 1);
                    }
                    parent?.addChild(token);
                    break;
                case AST.Node.NodeType.Prototype:
                    break;
                case AST.Node.NodeType.ExternStatement:
                    break;
                case AST.Node.NodeType.Function:
                    if (token.value == "{")
                    {
                        addLayer = false;
                    }
                    break;
                case AST.Node.NodeType.ArrayExpression:
                    addLayer = false;
                    break;
                default:
                    parent?.addChild(token);
                    break;
            }
            // DebugConsole.Write("pushing parent with node type of " + parent.nodeType + " to parent stack and parent parent of node type " + parent?.parent?.nodeType);
            delimParentStack.Push(parent);
            // DebugConsole.WriteAnsi("[purple]puhsing level to layer stack[/]");
            if (addLayer)
            {
                addLayerToNamedASTStack();
            }
            delimLevel++;
        }
        else if (token.type == Util.TokenType.DelimiterClose)
        {
            AST.Node delimParent = delimParentStack.Pop();
            bool removeLayer = true;
            switch (parent?.nodeType)
            {
                case AST.Node.NodeType.ArrayExpression:
                    removeLayer = false;
                    break;
                case AST.Node.NodeType.ForLoop:
                    if (token.value != ")")
                    {
                        break;
                    }
                    delimLevel--;
                    return (parent, delimLevel);
                case AST.Node.NodeType.WhileLoop:
                    if (token.value != ")")
                    {
                        break;
                    }
                    delimLevel--;
                    return (parent, delimLevel);
                case AST.Node.NodeType.IfStatement:
                    break;
                case AST.Node.NodeType.Function:
                    break;
                case AST.Node.NodeType.Prototype:
                    if (token.value == ")")
                    {
                        if (nextNonSpace().value == "{")
                        {
                            parent = new AST.Function((AST.Prototype)parent);
                            removeLayer = false;
                            delimLevel--;
                            return (parent, delimLevel);
                        }
                        else
                        {
                            nodes.Add(parent);
                            break;
                        }
                    }
                    else if (token.value == "]")
                    {
                        parent.addChild(token);
                        delimLevel--;
                        return (parent, delimLevel);
                    }
                    break;
                case AST.Node.NodeType.VariableDeclaration:
                    parent?.addChild(token);
                    delimLevel--;
                    return (parent, delimLevel);
                case AST.Node.NodeType.VariableAssignment:
                    parent?.addChild(token);
                    delimLevel--;
                    return (parent, delimLevel);
                default:
                    delimParent?.addChild(token);
                    break;
            }
            delimLevel--;
            if (removeLayer)
            {
                removeLayerFromASTStack();
                // DebugConsole.WriteAnsi("[purple]removing layer from ast stack[/]");
            }
            if (delimLevel == 0)
            {
                parent = new AST.Empty();
            }
            else
            {
                // DebugConsole.Write("setting parent to delim parent of node type " + delimParent?.nodeType + " with parent of node type " + delimParent?.parent?.nodeType);
                parent = delimParent.parent;
            }
        }

        return (parent, delimLevel);
    }


    public void parse()
    {
        currentTokenNum++;

        previousNode = nodes.Count > 0 && currentTokenNum > 0 ? nodes.Last() : null;

        //NOTE: handles imports and adding stuff
        if (previousNode != null && !previousNode.exportChecked)
        {
            // DebugConsole.Write("checking export for node of type " + previousNode.nodeType);
            previousNode?.checkExport();
        }

        List<Util.TokenType> expectedTypes = new List<Util.TokenType>();

        finalTokenNum = tokenList.Count();

        if (parent?.isExpression == false)
        {
            lastMajorParentNode = parent;
        }

        if (progressTask != null)
        {
            progressTask.Increment(1);
        }

        isFinishedParsing = currentTokenNum == finalTokenNum;

        if (isFinishedParsing)
        {
            return;
        }

        Util.Token token = tokenList[currentTokenNum];

        if (token.type != Util.TokenType.Space)
        {
            DebugConsole.Write($"token of value: {token.value} and type of {token.type} and parent of {parent?.nodeType} and delim level of {delimLevel} in file named {fileName} and previous node of type {previousNode?.nodeType}");
        }
        if (this.parent?.nodeType == AST.Node.NodeType.Empty)
        {
            previousNode?.addCode(token);
        }
        else
        {
            this.parent?.addCode(token);
        }

        prevLine = token.line;
        prevColumn = token.column;


        if (token.type == Util.TokenType.EOF)
        {
            this.isFinishedParsing = true;
            return;
        }
        else if (token.type == Util.TokenType.EOL)
        {
            if (singleLineComment)
            {
                singleLineComment = false;

                return;
            }

            if (delimLevel > 0)
            {
                while (parent?.newLineReset == true)
                {
                    parent = parent.parent;

                }

                return;
            }

            if (parent?.nodeType != AST.Node.NodeType.Function && parent?.nodeType != AST.Node.NodeType.IfStatement && parent?.nodeType != AST.Node.NodeType.ElseStatement && parent?.nodeType != AST.Node.NodeType.ForLoop /* && tokenList[tokenIndex - 1].value != "{" */ && delimLevel == 0)
            {

                parent = new AST.Empty();
                return;
            }
            else
            {

                return;
            }
        }

        if (token.value == Config.settings.general.comment.singleLine)
        {
            singleLineComment = true;

            return;
        }
        else if (token.value == Config.settings.general.comment.multiLineOpen)
        {
            multiLineComment = true;

            return;
        }
        else if (token.value == Config.settings.general.comment.multiLineClose)
        {
            multiLineComment = false;

            return;
        }

        if (singleLineComment || multiLineComment)
        {

            return;
        }

        if (expectedTypes != null)
        {
            checkToken(token, expectedTypes);
        }

        switch (token.type)
        {
            // case Util.TokenType.Space:
            //     parent?.addSpace(token);
            //     break;
            case Util.TokenType.Int:

                // if (parent.nodeType == AST.Node.NodeType.VariableAssignment)
                // {
                //     parent.addChild(token);
                //     break;
                // }
                new AST.NumberExpression(token, parent);
                break;
            case Util.TokenType.Double:
                new AST.NumberExpression(token, parent);
                break;
            case Util.TokenType.Operator:
                AST.Expression leftHand;
                if (previousNode == null)
                {
                    throw new Exception("previous node is null");
                }
                //check if previous node is an expression and throw an error if it isnt
                else if (!previousNode.isExpression)
                {
                    throw new Exception("expected expression before operator");
                }
                else
                {
                    leftHand = (AST.Expression)previousNode;
                }
                AST.BinaryExpression binExpr = new AST.BinaryExpression(leftHand, token, parent);
                parent = binExpr;
                return;

            case Util.TokenType.Keyword:
                (AST.Node keyParent, int keyDelimLevel) = parseKeyword(token, currentTokenNum, parent, delimLevel);
                //0 is the keyword AST.Node, 1 is the next token, and 2 is the next token index
                parent = keyParent;
                delimLevel = keyDelimLevel;
                return;
            case Util.TokenType.AssignmentOp:
                DebugConsole.Write("assignment op detected");
                if (parent?.nodeType == AST.Node.NodeType.VariableDeclaration)
                {
                    parent.addChild(token);
                }
                else
                {
                    DebugConsole.WriteAnsi("[purple]Creating var ass[/]");
                    AST.VariableAssignment varAss = new AST.VariableAssignment(token, parent);

                    parent = varAss;
                    return;
                    // VariableReAssignment varReAss = new VariableReAssignment(token);
                    // return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, varReAss, delimLevel: delimLevel);
                }
                break;
            case Util.TokenType.String:
                new AST.StringExpression(token, parent);

                return;
            case Util.TokenType.Modifier:
                switch (token.value)
                {
                    case "*":
                        parent = new AST.Dereference(token, parent);

                        return;
                    case "&":
                        parent = new AST.Reference(token, parent);
                        return;
                }
                break;
            case Util.TokenType.Special:
                //TODO: implement parsing of special chars
                if (token.value == ";")
                {
                    if (Config.settings.general.semiColon.mode != "None")
                    {
                        if (delimLevel > 0)
                        {
                            while (parent?.newLineReset == true)
                            {
                                parent = parent?.parent;
                            }

                            return;
                        }

                        if (parent?.nodeType != AST.Node.NodeType.Function && parent?.nodeType != AST.Node.NodeType.IfStatement && parent?.nodeType != AST.Node.NodeType.ElseStatement && parent?.nodeType != AST.Node.NodeType.ForLoop /* && tokenList[tokenIndex - 1].value != "{" */ && delimLevel == 0)
                        {

                            parent = parent?.parent;
                            return;
                        }
                        else
                        {

                            return;
                        }
                    }
                    else
                    {
                        throw ParserException.FactoryMethod("Illegal semi colon", "Remove the semi colon", token, parent);
                    }
                }
                else if (token.value == "->")
                {
                    AST.Dereference deRef = new AST.Dereference(token, parent);
                    parent = deRef;

                    return;
                }
                else if (token.value == "#")
                {
                    parent?.addChild(token);

                    return;
                }
                else
                {
                    parent?.addChild(token);
                    return;
                }
        }

        if (token.isDelim)
        {
            (AST.Node delimParentRet, int delimReturnLevel) = parseDelim(token, currentTokenNum, parent, delimLevel);

            DebugConsole.Write("delim returned parent of " + delimParentRet?.nodeType);
            parent = delimParentRet;
            delimLevel = delimReturnLevel;
            return;
            // return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, delimParent, delimLevel: delimRet[1]);
            // if (parent != null)
            //     return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent);
        }

        // DebugConsole.WriteAnsi("[green]parser debug below[/]");
        // printAST(nodes);
        // DebugConsole.WriteAnsi("[green]parser debug end[/]");

        return;
    }

    public Parser(List<Util.Token> tokenList, string fileName, string filePath, Spectre.Console.ProgressTask progressTask = null)
    {
        this.tokenList = tokenList;
        this.progressTask = progressTask;

        this.fileName = fileName;
        this.filePath = filePath;

        this.parentParser = Parser.getInstance();

        DebugConsole.WriteAnsi("[red]tokens[/]");
        foreach (Util.Token token in tokenList)
        {
            if (token.type != Util.TokenType.String)
            {
                DebugConsole.Write(token.value + " " + token.type);
            }
        }
        DebugConsole.WriteAnsi("[red]end tokens[/]");
        addLayerToNamedASTStack();

        if (progressTask != null)
        {
            progressTask.MaxValue = tokenList.Count();
        }
        this.parent = new AST.Empty();
    }


    public static List<Parser> startParsing(List<Util.Token> tokenList, string fileName, string filePath, Spectre.Console.ProgressTask task = null)
    {
        DebugConsole.WriteAnsi($"[red]fileName: {fileName}[/]");
        addInstance(tokenList, fileName, filePath, task);
        // getInstance().parse(tokenList, task);

        List<Parser> completedParsersList = new List<Parser>();

        Parser topParser = getInstance();
        while (parserStack.Count > 0)
        {
            if (topParser != getInstance())
            {
                topParser = getInstance();
            }
            if (topParser.isFinishedParsing)
            {
                completedParsersList.Add(removeInstance());
                if (parserStack.Count == 0)
                {
                    break;
                }
                topParser = getInstance();
                DebugConsole.WriteAnsi("[green]switching current parser to " + topParser.fileName + "[/]");
            }
            topParser.parse();
        }

        return completedParsersList;
    }

    //take in main file -> add to parser stack -> parse normally -> when finished parsing return the parser and remove it from the stack (add it to list of completed)
    //take in main file -> add to parser stack -> parse normally -> encounter an import -> create a new parser instance -> push it to the stack -> parse there -> give nodes back to parent parser -> allow parent parser to continue -> remove public nodes from the last thing -> individually generate each file -> link them together
}

