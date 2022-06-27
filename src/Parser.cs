//TODO: double check the previous node stuff for binexpr

using System.Text;
using static System.Text.Json.JsonSerializer;


public static class Parser
{
    public static List<ASTNode> nodes = new List<ASTNode>();
    public static List<Util.Token> tokenList;

    public static List<VariableAssignment> globalVarAss = new List<VariableAssignment>();

    public static Util.TokenType[] binaryExpectedTokens = { Util.TokenType.Number };
    public static Util.TokenType[] delimiterExpectedTokens = { Util.TokenType.Keyword };
    public static ASTNode.NodeType[] binaryExpectedNodes = { ASTNode.NodeType.NumberExpression, ASTNode.NodeType.BinaryExpression, ASTNode.NodeType.VariableExpression };

    public static int prevLine = 0;
    public static int prevColumn = 0;

    public static int ifFuncNum = 0;

    public static class topAST
    {
        public static List<ASTNode> primaryChildren = new List<ASTNode>();
    }

    public static void checkNode(ASTNode? node, ASTNode.NodeType[] expectedTypes)
    {
        if (node == null)
        {
            throw new ParserException($"expected a node at but got null", prevLine, prevColumn);
        }

        foreach (ASTNode.NodeType expectedNodeType in expectedTypes)
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

    public static void checkNode(ASTNode? node, ASTNode.NodeType[] expectedTypes, ParserException except)
    {
        if (node == null)
        {
            throw new ParserException($"expected a node at but got null", prevLine, prevColumn);
        }

        foreach (ASTNode.NodeType expectedNodeType in expectedTypes)
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

    public static string printBinary(BinaryExpression bin)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append($"{bin.nodeType} op: {bin.operatorType} lhs type: {bin.leftHand.nodeType} rhs type: {bin.rightHand.nodeType} binop children below:");
        stringBuilder.Append(printASTRet(bin.children));

        return stringBuilder.ToString();
    }

    public static string printFunc(FunctionAST func)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append($"{func.nodeType} name: {func.prototype.name} args: {Serialize(func.prototype.arguments.ToList())} body start: ");

        stringBuilder.Append(printASTRet(func.body));

        stringBuilder.Append("function body end");

        return stringBuilder.ToString();
    }

    public static string printFuncCall(FunctionCall funcCall)
    {
        return $"{funcCall.nodeType} with name of {funcCall.functionName} and args of {String.Join(", ", funcCall.args)}";
    }

    public static string printVarAss(VariableAssignment varAss)
    {
        return $"{varAss.nodeType} with type of {varAss?.type.value} and assignmentop of {varAss.assignmentOp} and name of {varAss.name} and mutability of {varAss.mutable} and value of {varAss.strValue}";
    }

    public static string printProto(PrototypeAST proto)
    {
        return $"{proto.nodeType} with name of {proto.name}";
    }

    public static string printIfStat(IfStatement ifStat)
    {
        return $"if statement with expression of {printBinary(ifStat.expression)} and body of ( {printASTRet(ifStat.thenFunc.body)} ) body end | else statement: {printElseStat(ifStat.elseStat)}";
    }

    public static string printElseStat(ElseStatement elseStat)
    {
        return $"else statement with body of ( {printASTRet(elseStat.elseFunc.body)} )";
    }

    public static string printForLoop(ForLoop forLoop)
    {
        return $"For loop with iteration object of {forLoop.iterationObject} and index obj of {printASTRet(new List<ASTNode>() { forLoop.index })} complexity of {forLoop.complex} and body of ( {printASTRet(forLoop.body)} ) body end";
    }

    public static string printASTRet(List<ASTNode> nodesPrint)
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach (ASTNode node in nodesPrint)
        {
            switch (node.nodeType)
            {
                case ASTNode.NodeType.BinaryExpression:
                    BinaryExpression bin = (BinaryExpression)node;
                    stringBuilder.Append(printBinary(bin));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.Function:
                    FunctionAST func = (FunctionAST)node;
                    stringBuilder.Append(printFunc(func));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.FunctionCall:
                    FunctionCall funcCall = (FunctionCall)node;
                    stringBuilder.Append(printFuncCall(funcCall));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.VariableAssignment:
                    VariableAssignment varAss = (VariableAssignment)node;
                    stringBuilder.Append(printVarAss(varAss));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.Prototype:
                    PrototypeAST proto = (PrototypeAST)node;
                    stringBuilder.Append(printProto(proto));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.NumberExpression:
                    NumberExpression numExp = (NumberExpression)node;
                    stringBuilder.Append($"{numExp.nodeType} of value {numExp.value}");
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.IfStatement:
                    IfStatement ifStat = (IfStatement)node;
                    stringBuilder.Append(printIfStat(ifStat));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.ForLoop:
                    ForLoop forLoop = (ForLoop)node;
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

    public static void printAST(List<ASTNode> nodesPrint)
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach (ASTNode node in nodesPrint)
        {
            switch (node.nodeType)
            {
                case ASTNode.NodeType.BinaryExpression:
                    BinaryExpression bin = (BinaryExpression)node;
                    stringBuilder.Append(printBinary(bin));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.Function:
                    FunctionAST func = (FunctionAST)node;
                    stringBuilder.Append(printFunc(func));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.FunctionCall:
                    FunctionCall funcCall = (FunctionCall)node;
                    stringBuilder.Append(printFuncCall(funcCall));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.VariableAssignment:
                    VariableAssignment varAss = (VariableAssignment)node;
                    stringBuilder.Append(printVarAss(varAss));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.Prototype:
                    PrototypeAST proto = (PrototypeAST)node;
                    stringBuilder.Append(printProto(proto));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.IfStatement:
                    IfStatement ifStat = (IfStatement)node;
                    stringBuilder.Append(printIfStat(ifStat));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.ForLoop:
                    ForLoop forLoop = (ForLoop)node;
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

    public static List<dynamic> parseKeyword(Util.Token token, int tokenIndex, ASTNode? parent = null, int delimLevel = 0)
    {
        List<dynamic> ret = new List<dynamic>();
        Util.Token nextToken = tokenList[tokenIndex + 1];
        int nextTokenIndex = tokenIndex + 1;

        switch (parent?.nodeType)
        {
            case ASTNode.NodeType.Prototype:
                PrototypeAST proto = (PrototypeAST)parent;
                proto.addItem(token);
                return new List<dynamic>() { parent, delimLevel };
            case ASTNode.NodeType.VariableAssignment:
                parent.addChild(token);
                return new List<dynamic>() { parent, delimLevel };
        }

        if (token.value == "var")
        {
            VariableAssignment varAss = new VariableAssignment(token, true);
            return new List<dynamic>() { varAss, delimLevel };
        }
        else if (token.value == "const")
        {
            VariableAssignment constAss = new VariableAssignment(token, false);
            return new List<dynamic>() { constAss, delimLevel };
        }
        else if (token.value == "if")
        {
            IfStatement ifStat = new IfStatement(token, parent);
            return new List<dynamic>() { ifStat, delimLevel };
        }
        else if (token.value == "else")
        {
            IfStatement ifParent = (IfStatement)parent;
            return new List<dynamic>() { ifParent.elseStat, delimLevel };
        }
        else if (token.value == "for")
        {
            ForLoop forLoop = new ForLoop(token, parent);
            return new List<dynamic>() { forLoop, delimLevel };
        }
        else if (token.value[0] == '@')
        {
            PrototypeAST proto = new PrototypeAST(token);
            return new List<dynamic>() { proto, delimLevel };
        }
        else if (token.value.EndsWith("!"))
        {
            //treat it as a builtin call
            FunctionCall builtinCall = new FunctionCall(token, null, true, parent);
            return new List<dynamic>() { builtinCall, delimLevel };
        }
        else if (nextToken.type == Util.TokenType.ParenDelimiterOpen)
        {
            FunctionCall funcCall = new FunctionCall(token, null, false, parent);
            return new List<dynamic>() { funcCall, delimLevel };
        }

        new VariableExpression(token, parent);
        return new List<dynamic>() { parent, delimLevel };
    }

    public static List<dynamic> parseDelim(Util.Token token, int tokenIndex, ASTNode? parent = null, int delimLevel = 0)
    {
        switch (token.type)
        {
            case Util.TokenType.ParenDelimiterOpen:
                switch (parent?.nodeType)
                {
                    case ASTNode.NodeType.ForLoop:
                        parent.addChild(token);
                        break;

                }
                delimLevel++;
                break;
            case Util.TokenType.BrackDelimiterOpen:
                switch (parent?.nodeType)
                {
                    // case ASTNode.NodeType.Prototype:
                    //     parent = new FunctionAST((PrototypeAST)parent);
                    //     break;
                    case ASTNode.NodeType.IfStatement:
                        parent.addChild(token);
                        break;
                }

                delimLevel++;
                break;
            case Util.TokenType.SquareDelimiterOpen:
                switch (parent?.nodeType)
                {
                    case ASTNode.NodeType.FunctionCall:
                        break;
                }
                delimLevel++;
                break;
            case Util.TokenType.SquareDelimiterClose:
                delimLevel--;
                if (delimLevel == 0)
                {
                    parent = null;
                }
                else if (parent != null)
                {
                    parent = parent.parent;
                }
                break;
            case Util.TokenType.ParenDelimiterClose:
                delimLevel--;
                if (parent.nodeType == ASTNode.NodeType.Prototype)
                {
                    parent = new FunctionAST((PrototypeAST)parent, new List<ASTNode>());
                    break;
                }
                if (delimLevel == 0)
                {
                    switch (parent.nodeType)
                    {
                        case ASTNode.NodeType.IfStatement:
                            break;
                    }
                    parent = null;
                }
                else if (parent != null)
                {
                    parent = parent.parent;
                    Console.WriteLine($"set parent delim to parent.parent (parent.parent is type {parent?.nodeType})");
                }
                break;
            case Util.TokenType.BrackDelimiterClose:
                // if (delimLevel == 0)
                // {
                //     parent = null;
                // }
                // if (parent != null)
                // {
                //     parent = parent.parent;
                // }
                delimLevel--;
                break;
        }
        return new List<dynamic>() { parent, delimLevel };
    }

    public static bool parseTokenRecursive(Util.Token token, int tokenIndex, ASTNode? parent = null, Util.TokenType[]? expectedTypes = null, int delimLevel = 0)
    {
        prevLine = token.line;
        prevColumn = token.column;

        // Console.WriteLine($"token of value: {token.value} and parent of {parent?.nodeType}");
        ASTNode? previousNode = nodes.Count > 0 ? nodes.Last() : null;

        if (token.type == Util.TokenType.EOF)
        {
            return true;
        }
        else if (token.type == Util.TokenType.EOL)
        {
            if (parent?.nodeType != ASTNode.NodeType.Function && parent?.nodeType != ASTNode.NodeType.IfStatement && parent?.nodeType != ASTNode.NodeType.ElseStatement)
            {
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, null, delimLevel: delimLevel);
            }
            return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent, delimLevel: delimLevel);
        }

        if (expectedTypes != null)
        {
            checkToken(token, expectedTypes);
        }

        switch (token.type)
        {
            case Util.TokenType.Number:

                if (parent.nodeType == ASTNode.NodeType.VariableAssignment)
                {
                    parent.addChild(token);
                    break;
                }
                new NumberExpression(token, parent);
                break;

            case Util.TokenType.Operator:
                BinaryExpression binExpr = new BinaryExpression(token, previousNode, tokenList[tokenIndex + 1], parent);
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, binExpr, binaryExpectedTokens, delimLevel: delimLevel);

            case Util.TokenType.Keyword:
                List<dynamic> keywordRet = parseKeyword(token, tokenIndex, parent, delimLevel);
                //0 is the keyword ASTNode, 1 is the next token, and 2 is the next token index
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, keywordRet[0], delimLevel: keywordRet[1]);

            case Util.TokenType.AssignmentOp:
                if (parent?.nodeType == ASTNode.NodeType.VariableAssignment)
                {
                    parent.addChild(token);
                }
                else
                {
                    throw new ParserException($"illegal assignment op", token);
                }
                break;
            case Util.TokenType.String:
                new StringExpression(token, parent);
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent, delimLevel: delimLevel);
        }

        if (token.isDelim)
        {
            List<dynamic> delimRet = parseDelim(token, tokenIndex, parent, delimLevel);
            ASTNode delimParent = delimRet[0];
            return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, delimParent, delimLevel: delimRet[1]);
            // if (parent != null)
            //     return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent);
        }
        return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent, delimLevel: delimLevel);

    }

    public static List<ASTNode> beginParse(List<Util.Token> _tokenList)
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
        VariableAssignment newLineAss = new VariableAssignment(newLineAssToken, false);
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

        PrototypeAST printProto = new PrototypeAST(printToken, printProtoArgs);
        nodes.Insert(0, printProto);

        List<Util.Token> printlnProtoArgs = new List<Util.Token>();

        printlnProtoArgs.Add(new Util.Token(Util.TokenType.Keyword, "double", 0, 0));
        printlnProtoArgs.Add(new Util.Token(Util.TokenType.Keyword, "x", 0, 0));
        Util.Token printlnToken = new Util.Token(Util.TokenType.Keyword, "@println", 0, 0);

        PrototypeAST printlnProto = new PrototypeAST(printlnToken, printlnProtoArgs);
        nodes.Insert(0, printlnProto);


    }
}

