using System.Text;
using static System.Text.Json.JsonSerializer;
using System.Linq;

public static class Parser
{
    public static List<ASTNode> nodes = new List<ASTNode>();
    public static List<Util.Token> tokenList;

    public static Util.TokenType[] binaryExpectedTokens = { Util.TokenType.Number };
    public static Util.TokenType[] delimiterExpectedTokens = { Util.TokenType.Keyword };
    public static ASTNode.NodeType[] binaryExpectedNodes = { ASTNode.NodeType.NumberExpression, ASTNode.NodeType.BinaryExpression };

    public static class topAST
    {
        public static List<ASTNode> primaryChildren = new List<ASTNode>();
    }

    public static void checkNode(ASTNode? node, ASTNode.NodeType[] expectedTypes)
    {
        if (node == null)
        {
            throw new ArgumentException($"expected a node at (line and column goes here) but got null");
        }

        foreach (ASTNode.NodeType expectedNodeType in expectedTypes)
        {
            if (node.nodeType != expectedNodeType && expectedNodeType == expectedTypes.Last())
            {
                throw new ArgumentException($"expected type {string.Join(", ", expectedTypes)} but got {node.nodeType}");
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
            throw new ArgumentException($"expected a token at {token.line}:{token.column} but got null");
        }

        if (expectedTypes != null)
        {
            foreach (Util.TokenType expectedTokenType in expectedTypes)
            {
                if (token.type != expectedTokenType && expectedTokenType == expectedTypes.Last())
                {
                    throw new ArgumentException($"expected token of type {string.Join(", ", expectedTypes)} but got {token.type} at {token.line}:{token.column}");
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
                throw new ArgumentException($"expected token of type {expectedType} but got {token.type} at {token.line}:{token.column}");
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
        return $"{varAss.nodeType} with type of {varAss.type.value} and assignmentop of {varAss.assignmentOp} and name of {varAss.name} and mutability of {varAss.mutable}";
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
                break;
            case ASTNode.NodeType.VariableAssignment:
                parent.addChild(token);
                break;
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
        else if (token.value[0] == '@')
        {
            PrototypeAST proto = new PrototypeAST(token.value.Substring(1));
            return new List<dynamic>() { proto, delimLevel };
        }
        else if (token.value.EndsWith("!"))
        {
            //treat it as a function call
            //token would be the name, next token would be delim, so we grab all tokens starting from the one after that until final delim
            FunctionCall builtinCall = new FunctionCall(token, null, true, parent);
            return new List<dynamic>() { builtinCall, delimLevel };
        }
        else if (nextToken.type == Util.TokenType.ParenDelimiterOpen)
        {
            FunctionCall funcCall = new FunctionCall(token, null, false, parent);
            return new List<dynamic>() { funcCall, delimLevel };
        }

        return new List<dynamic>() { parent, delimLevel };
    }

    public static List<dynamic> parseDelim(Util.Token token, int tokenIndex, ASTNode? parent = null, int delimLevel = 0)
    {
        switch (token.type)
        {
            case Util.TokenType.ParenDelimiterOpen:
                // switch (parent?.nodeType)
                // {
                //     case ASTNode.NodeType.Prototype:
                //     
                // }
                delimLevel++;
                break;
            case Util.TokenType.BrackDelimiterOpen:
                // switch (parent?.nodeType)
                // {
                //     case ASTNode.NodeType.Prototype:
                //         parent = new FunctionAST((PrototypeAST)parent);
                //         break;
                // }
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
                    parent = null;
                }
                else if (parent != null)
                {
                    parent = parent.parent;
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

        ASTNode? previousNode = nodes.Count > 0 ? nodes.Last() : null;

        if (token.type == Util.TokenType.EOF)
        {
            return true;
        }
        else if (token.type == Util.TokenType.EOL)
        {
            return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent, delimLevel: delimLevel);
        }

        if (expectedTypes != null)
        {
            checkToken(token, expectedTypes);
        }

        switch (token.type)
        {
            case Util.TokenType.Number:
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
                if (parent.nodeType == ASTNode.NodeType.VariableAssignment)
                {
                    parent.addChild(token);
                }
                else
                {
                    throw new Exception($"illegal assignment op at {token.line}:{token.column}");
                }
                break;
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

        parseTokenRecursive(tokenList[0], 0);

        List<Util.Token> protoArgs = new List<Util.Token>();

        protoArgs.Add(new Util.Token(Util.TokenType.Keyword, "string", 0, 0));
        protoArgs.Add(new Util.Token(Util.TokenType.Keyword, "format", 0, 0));
        protoArgs.Add(new Util.Token(Util.TokenType.Keyword, "double", 0, 0));
        protoArgs.Add(new Util.Token(Util.TokenType.Keyword, "x", 0, 0));

        PrototypeAST printProto = new PrototypeAST("printf", protoArgs);
        nodes.Insert(0, printProto);

        Console.WriteLine("Parser debug info below");
        printAST(nodes);
        Console.WriteLine("Parser debug info finsihed");

        return nodes;
    }
}
