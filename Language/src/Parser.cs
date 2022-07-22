//TODO: double check the previous node stuff for binexpr

using System.Text;
using static System.Text.Json.JsonSerializer;


public static class Parser
{
    public static List<AST.Node> nodes = new List<AST.Node>();
    public static List<Util.Token> tokenList;

    public static List<AST.VariableAssignment> globalVarAss = new List<AST.VariableAssignment>();

    public static Util.TokenType[] binaryExpectedTokens = { Util.TokenType.Int, Util.TokenType.Keyword };
    public static Util.TokenType[] delimiterExpectedTokens = { Util.TokenType.Keyword };
    public static AST.Node.NodeType[] binaryExpectedNodes = { AST.Node.NodeType.NumberExpression, AST.Node.NodeType.BinaryExpression, AST.Node.NodeType.VariableExpression, AST.Node.NodeType.PhiVariable };

    public static Dictionary<string, AST.Function> declaredFunctionDict = new Dictionary<string, AST.Function>();
    public static Dictionary<string, AST.VariableAssignment> declaredGlobalsDict = new Dictionary<string, AST.VariableAssignment>();

    public static int prevLine = 0;
    public static int prevColumn = 0;

    public static int ifFuncNum = 0;

    //NOTE: below can be used to add user defined types (structs/classes)
    public static List<string> typeList = new List<string>() { "double", "int", "string" };

    public static void checkNode(AST.Node? node, AST.Node.NodeType[] expectedTypes)
    {
        if (node == null)
        {
            throw new ParserException($"expected a node at but got null", prevLine, prevColumn);
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

    public static void checkNode(AST.Node? node, AST.Node.NodeType[] expectedTypes, ParserException except)
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

    public static void checkToken(Util.Token? token, Util.TokenType[]? expectedTypes = null, Util.TokenType? expectedType = null)
    {
        if (token == null)
        {
            throw new ParserException($"expected a token but got null", prevLine, prevColumn);
        }

        if (expectedTypes != null)
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
        else
        {
            if (token.type != expectedType)
            {
                throw new ParserException($"expected token of type {expectedType} but got {token.type}", token);
            }
        }
    }

    public static string printBinary(AST.BinaryExpression bin)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append($"{bin.nodeType} op: {bin.operatorType} lhs type: {bin.leftHand.nodeType} rhs type: {bin.rightHand.nodeType} binop children below:");
        stringBuilder.Append(printASTRet(bin.children));

        return stringBuilder.ToString();
    }

    public static string printFunc(AST.Function func)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append($"{func.nodeType} name: {func.prototype.name} args: {Serialize(func.prototype.arguments.ToList())} body start: ");

        stringBuilder.Append(printASTRet(func.body));

        stringBuilder.Append("function body end");

        return stringBuilder.ToString();
    }

    public static string printFuncCall(AST.FunctionCall funcCall)
    {
        return $"{funcCall.nodeType} with name of {funcCall.functionName} and args of {String.Join(", ", funcCall.args)}";
    }

    public static string printVarAss(AST.VariableAssignment varAss)
    {
        if (!varAss.reassignment)
        {
            return $"{varAss.nodeType} with type of {varAss?.type.value} and assignmentop of {varAss?.assignmentOp} and name of {varAss?.name} and mutability of {varAss.mutable} and value of {varAss.strValue}";
        }
        else
        {
            return $"{varAss.nodeType} with name of {varAss.name} and children of [{printASTRet(varAss.children)}]";
        }
    }

    public static string printProto(AST.Prototype proto)
    {
        return $"{proto.nodeType} with name of {proto.name}";
    }

    public static string printIfStat(AST.IfStatement ifStat)
    {
        return $"if statement with expression of {printBinary(ifStat.expression)} and body of ( {printASTRet(ifStat.thenFunc.body)} ) body end | else statement: {printElseStat(ifStat.elseStat)}";
    }

    public static string printElseStat(AST.ElseStatement elseStat)
    {
        return $"else statement with body of ( {printASTRet(elseStat.elseFunc.body)} )";
    }

    public static string printForLoop(AST.ForLoop forLoop)
    {
        return $"For loop with iteration object of {forLoop.iterationObject} and index obj of {printPhiVar(forLoop.index)} complexity of {forLoop.complex} and body of ( {printASTRet(forLoop.body)} ) body end";
    }

    public static string printPhiVar(AST.PhiVariable phiVar)
    {
        return $"phi variable with type of {phiVar.type.value} and name of {phiVar.name} and value of {phiVar.value}";
    }

    public static string printASTRet(List<AST.Node> nodesPrint)
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

    public static void printAST(List<AST.Node> nodesPrint)
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

        Console.WriteLine(stringBuilder);
    }


    public static List<Util.Token> getTokensUntil(int startIndex, Util.TokenType stopType)
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

    public static List<dynamic> parseKeyword(Util.Token token, int tokenIndex, AST.Node? parent = null, int delimLevel = 0)
    {
        List<dynamic> ret = new List<dynamic>();
        Util.Token nextToken = tokenList[tokenIndex + 1];
        int nextTokenIndex = tokenIndex + 1;


        if (token.value == Config.settings.variable.declaration.keyword.mutable)
        {
            AST.VariableAssignment varAss = new AST.VariableAssignment(token, true);
            return new List<dynamic>() { varAss, delimLevel };
        }
        else if (token.value == Config.settings.variable.declaration.keyword.constant)
        {
            AST.VariableAssignment constAss = new AST.VariableAssignment(token, false);
            return new List<dynamic>() { constAss, delimLevel };
        }
        else if (token.value == Config.settings.general.import.keyword)
        {
            AST.ImportStatement importStatement = new AST.ImportStatement(token);
            return new List<dynamic>() { importStatement, delimLevel };
        }
        else if (token.value == Config.settings.general.import.ignore.keyword)
        {
            //TODO: add code here to implement "private"
            //NOTE: I can do this by throwing errors if a private thing is called outside of its file? I will have to store the origin files of stuff
        }
        else if (token.value == "if")
        {
            AST.IfStatement ifStat = new AST.IfStatement(token, parent);
            return new List<dynamic>() { ifStat, delimLevel };
        }
        else if (token.value == "else")
        {
            AST.IfStatement ifParent = (AST.IfStatement)parent;
            return new List<dynamic>() { ifParent.elseStat, delimLevel };
        }
        else if (token.value == "for")
        {
            AST.ForLoop forLoop = new AST.ForLoop(token, parent);
            return new List<dynamic>() { forLoop, delimLevel };
        }
        else if (Config.settings.function.declaration.marker.word && token.value == Config.settings.function.declaration.marker.value)
        {
            AST.Prototype proto = new AST.Prototype(token);
            return new List<dynamic>() { proto, delimLevel };
        }
        else if (!Config.settings.function.declaration.marker.word && token.value[0].ToString() == Config.settings.function.declaration.marker.value)
        {
            AST.Prototype proto = new AST.Prototype(token);
            return new List<dynamic>() { proto, delimLevel };
        }
        else if (Config.settings.function.calling.builtin.marker.location == "end" && token.value.EndsWith(Config.settings.function.calling.builtin.marker.value))
        {
            //treat it as a builtin call
            AST.FunctionCall builtinCall = new AST.FunctionCall(token, null, true, parent, false);
            return new List<dynamic>() { builtinCall, delimLevel };
        }
        else if (Config.settings.function.calling.builtin.marker.location == "beginning" && token.value.StartsWith(Config.settings.function.calling.builtin.marker.value))
        {
            //treat it as a builtin call
            AST.FunctionCall builtinCall = new AST.FunctionCall(token, null, true, parent, false);
            return new List<dynamic>() { builtinCall, delimLevel };
        }
        else if (nextToken.value == Config.settings.function.calling.args.delimeters[0])
        {
            AST.FunctionCall funcCall = new AST.FunctionCall(token, null, false, parent);
            return new List<dynamic>() { funcCall, delimLevel };
        }

        else if (parent?.nodeType == AST.Node.NodeType.ForLoop)
        {
            AST.ForLoop forLoop = (AST.ForLoop)parent;
            if (!forLoop.isBody)
            {
                parent.addChild(token);
                return new List<dynamic>() { parent, delimLevel };
            }
        }

        switch (parent?.nodeType)
        {
            case AST.Node.NodeType.Prototype:
                AST.Prototype proto = (AST.Prototype)parent;
                proto.addItem(token);
                return new List<dynamic>() { parent, delimLevel };
            case AST.Node.NodeType.VariableAssignment:
                AST.VariableAssignment varAss = (AST.VariableAssignment)parent;
                if (varAss.reassignment)
                {
                    break;
                }
                parent.addChild(token);
                return new List<dynamic>() { parent, delimLevel };
            case AST.Node.NodeType.ImportStatement:
                parent.addChild(token);
                return new List<dynamic>() { parent.parent, delimLevel };
        }



        new AST.VariableExpression(token, parent);
        return new List<dynamic>() { parent, delimLevel };
    }

    public static List<dynamic> parseDelim(Util.Token token, int tokenIndex, AST.Node? parent = null, int delimLevel = 0)
    {

        if (token.type == Util.TokenType.DelimiterOpen)
        {
            switch (parent?.nodeType)
            {
                default:
                    parent?.addChild(token);
                    break;
            }
            delimLevel++;
        }
        else if (token.type == Util.TokenType.DelimiterClose)
        {
            switch (parent?.nodeType)
            {
                case AST.Node.NodeType.ForLoop:
                    delimLevel--;
                    return new List<dynamic>() { parent, delimLevel };
                    break;
                case AST.Node.NodeType.IfStatement:
                    break;
                case AST.Node.NodeType.Function:
                    break;
                case AST.Node.NodeType.Prototype:
                    parent = new AST.Function((AST.Prototype)parent);
                    delimLevel--;
                    return new List<dynamic>() { parent, delimLevel };
                    break;
                case AST.Node.NodeType.FunctionCall:
                    break;
                default:
                    parent?.addChild(token);
                    break;
            }
            delimLevel--;
            if (delimLevel == 0)
            {
                parent = null;
            }
            else if (parent != null)
            {
                parent = parent.parent;
            }
        }

        return new List<dynamic>() { parent, delimLevel };

    }

    public static bool parseTokenRecursive(Util.Token token, int tokenIndex, AST.Node? parent = null, Util.TokenType[]? expectedTypes = null, int delimLevel = 0)
    {
        prevLine = token.line;
        prevColumn = token.column;

        // Console.WriteLine($"token of value: {token.value} and parent of {parent?.nodeType}");
        AST.Node? previousNode = nodes.Count > 0 && tokenIndex > 0 ? nodes.Last() : null;

        if (token.type == Util.TokenType.EOF)
        {
            return true;
        }
        else if (token.type == Util.TokenType.EOL)
        {
            if (delimLevel > 0)
            {
                while (parent.newLineReset == true)
                {
                    parent = parent.parent;
                }
            }

            else if (parent?.nodeType != AST.Node.NodeType.Function && parent?.nodeType != AST.Node.NodeType.IfStatement && parent?.nodeType != AST.Node.NodeType.ElseStatement && parent?.nodeType != AST.Node.NodeType.ForLoop /* && tokenList[tokenIndex - 1].value != "{" */ && delimLevel == 0)
            {
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, null, delimLevel: delimLevel);
            }
            else
            {
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent, delimLevel: delimLevel);
            }
        }

        if (expectedTypes != null)
        {
            checkToken(token, expectedTypes);
        }

        switch (token.type)
        {
            case Util.TokenType.Int:

                if (parent.nodeType == AST.Node.NodeType.VariableAssignment)
                {
                    parent.addChild(token);
                    break;
                }
                new AST.NumberExpression(token, parent);
                break;

            case Util.TokenType.Operator:
                AST.BinaryExpression binExpr = new AST.BinaryExpression(token, previousNode, parent);
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, binExpr, binaryExpectedTokens, delimLevel: delimLevel);

            case Util.TokenType.Keyword:
                List<dynamic> keywordRet = parseKeyword(token, tokenIndex, parent, delimLevel);
                //0 is the keyword AST.Node, 1 is the next token, and 2 is the next token index
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, keywordRet[0], delimLevel: keywordRet[1]);

            case Util.TokenType.AssignmentOp:
                if (parent?.nodeType == AST.Node.NodeType.VariableAssignment)
                {
                    parent.addChild(token);
                }
                else
                {
                    AST.VariableAssignment varAss = new AST.VariableAssignment(token, true, parent);
                    return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, varAss, delimLevel: delimLevel);
                    // VariableReAssignment varReAss = new VariableReAssignment(token);
                    // return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, varReAss, delimLevel: delimLevel);
                }
                break;
            case Util.TokenType.String:
                new AST.StringExpression(token, parent);
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent, delimLevel: delimLevel);
        }

        if (token.isDelim)
        {
            List<dynamic> delimRet = parseDelim(token, tokenIndex, parent, delimLevel);
            AST.Node delimParent = delimRet[0];
            return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, delimParent, delimLevel: delimRet[1]);
            // if (parent != null)
            //     return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent);
        }
        return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent, delimLevel: delimLevel);

    }

    public static List<AST.Node> beginParse(List<Util.Token> _tokenList)
    {
        tokenList = _tokenList;

        addLanguageBuiltins();

        parseTokenRecursive(tokenList[0], 0);


        Console.WriteLine("Parser debug info below");
        printAST(nodes);
        Console.WriteLine("Parser debug info finsihed");

        return nodes;
    }

    public static void addLanguageBuiltins()
    {
        Util.Token newLineAssToken = new Util.Token(Util.TokenType.Keyword, "const", 0, 0);
        AST.VariableAssignment newLineAss = new AST.VariableAssignment(newLineAssToken, false);
        newLineAss.addChild(new Util.Token(Util.TokenType.Keyword, "string", 0, 0));
        newLineAss.addChild(new Util.Token(Util.TokenType.Keyword, "nl", 0, 0));
        newLineAss.addChild(new Util.Token(Util.TokenType.AssignmentOp, "=", 0, 0));
        newLineAss.addChild(new Util.Token(Util.TokenType.String, "\"\n\"", 0, 0));

        List<Util.Token> printProtoArgs = new List<Util.Token>();

        printProtoArgs.Add(new Util.Token(Util.TokenType.Keyword, "string", 0, 0));
        printProtoArgs.Add(new Util.Token(Util.TokenType.Keyword, "format", 0, 0));
        printProtoArgs.Add(new Util.Token(Util.TokenType.Keyword, "double", 0, 0));
        printProtoArgs.Add(new Util.Token(Util.TokenType.Keyword, "x", 0, 0));
        Util.Token printToken = new Util.Token(Util.TokenType.Keyword, "@printf", 0, 0);

        AST.Prototype printProto = new AST.Prototype(printToken, printProtoArgs);
        nodes.Insert(0, printProto);

        List<Util.Token> printlnProtoArgs = new List<Util.Token>();

        printlnProtoArgs.Add(new Util.Token(Util.TokenType.Keyword, "double", 0, 0));
        printlnProtoArgs.Add(new Util.Token(Util.TokenType.Keyword, "x", 0, 0));
        Util.Token printlnToken = new Util.Token(Util.TokenType.Keyword, "@println", 0, 0);

        AST.Prototype printlnProto = new AST.Prototype(printlnToken, printlnProtoArgs);
        nodes.Insert(0, printlnProto);


    }
}

