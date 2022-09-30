//TODO: double check the previous node stuff for binexpr

using System.Text;
using static System.Text.Json.JsonSerializer;
using Spectre.Console;

public static class Parser
{
    public static List<AST.Node> nodes = new List<AST.Node>();
    public static List<Util.Token> tokenList;

    public static List<AST.VariableDeclaration> globalVarAss = new List<AST.VariableDeclaration>();

    public static Util.TokenType[] binaryExpectedTokens = { Util.TokenType.Int, Util.TokenType.Keyword };
    public static Util.TokenType[] delimiterExpectedTokens = { Util.TokenType.Keyword };
    public static AST.Node.NodeType[] binaryExpectedNodes = { AST.Node.NodeType.NumberExpression, AST.Node.NodeType.BinaryExpression, AST.Node.NodeType.VariableExpression, AST.Node.NodeType.PhiVariable };

    public static Dictionary<string, AST.Function> declaredFunctionDict = new Dictionary<string, AST.Function>();
    public static Dictionary<string, AST.VariableDeclaration> declaredGlobalsDict = new Dictionary<string, AST.VariableDeclaration>();

    public static int prevLine = 0;
    public static int prevColumn = 0;

    public static int ifFuncNum = 0;

    //NOTE: below can be used to add user defined types (structs/classes)
    public static List<string> typeList = new List<string>() { "double", "float", "string" };

    //NOTE: below are all for the while loop func
    public static int finalTokenNum = 0;
    public static int currentTokenNum = 0;
    public static bool isFinishedParsing = false;
    public static Stack<AST.Node> delimParentStack = new Stack<AST.Node>();

    public static AST.Node lastMajorParentNode = null;

    public static AST.Node parent;

    public static string[] binaryMathOps = { "+", "-", "*", "/" };

    public static bool isType(Util.Token token)
    {
        if (typeList.Contains(token.value))
        {
            return true;
        }
        (bool isInt, int bits) = checkInt(token.value);
        return isInt;
    }

    public static bool isType(string value)
    {
        if (typeList.Contains(value))
        {
            return true;
        }
        (bool isInt, int bits) = checkInt(value);
        return isInt;
    }

    public static (bool, int) checkInt(string value)
    {
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

    public static void checkToken(Util.Token? token, List<Util.TokenType>? expectedTypes = null, Util.TokenType? expectedType = null)
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
        stringBuilder.Append($"{bin.nodeType} op: {bin.operatorType} lhs type: {bin.leftHand.nodeType} rhs type: {bin?.rightHand?.nodeType} binop children below:");
        stringBuilder.Append(printASTRet(bin?.children));

        return stringBuilder.ToString();
    }

    public static string printFunc(AST.Function func)
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (func.prototype.arguments.Count() > 0)
        {

            stringBuilder.Append($"first arg type: {printType(func.prototype.arguments.ElementAt(0).Value)}");
        }
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
        return $"{varAss?.nodeType} with name of {varAss?.varExpr?.value} and children of [{printASTRet(varAss?.children)}]";
    }

    public static string printProto(AST.Prototype proto)
    {
        return $"{proto.nodeType} with name of {proto.name}";
    }

    public static string printIfStat(AST.IfStatement ifStat)
    {
        return $"if statement with expression of {printBinary(ifStat.declaration.expression)} and body of ( {printASTRet(ifStat.thenBody)} ) body end | else statement: {printElseStat(ifStat.elseStat)}";
    }

    public static string printElseStat(AST.ElseStatement elseStat)
    {
        return $"else statement with body of ( {printASTRet(elseStat.elseBody)} )";
    }

    public static string printForLoop(AST.ForLoop forLoop)
    {
        // return $"For loop with iteration object of {forLoop.iterationObject} and index obj of {printPhiVar(forLoop.index)} complexity of {forLoop.complex} and body of ( {printASTRet(forLoop.body)} ) body end";
        return $"";
    }

    public static string printType(AST.Type type)
    {
        return $"type with name of {type.value} and is array of {type.isArray}";
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

        DebugConsole.Write(stringBuilder);
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
    public static Util.Token nextNonSpace()
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

    public static (Util.Token, int) nextNonSpace(int startIndex)
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

    public static Util.Token nextNonSpecial(int startIndex)
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

    public static (AST.Node, int) parseKeyword(Util.Token token, int tokenIndex, AST.Node? parent = null, int delimLevel = 0)
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
                return (parent.parent, delimLevel);

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
        if (parent?.nodeType != AST.Node.NodeType.VariableExpression)
        {
            DebugConsole.Write("returning var expr");
            return (varExpr, delimLevel);
        }
        return (parent, delimLevel);
    }

    public static (AST.Node parent, int delimLevel) parseDelim(Util.Token token, int tokenIndex, AST.Node? parent = null, int delimLevel = 0)
    {
        if (token.type == Util.TokenType.DelimiterOpen)
        {
            if (token.value == "[" && parent?.nodeType != AST.Node.NodeType.VariableDeclaration)
            {
                parent = new AST.IndexReference(token, parent);
                delimParentStack.Push(parent);
                delimLevel++;
                return (parent, delimLevel);
            }
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
                default:
                    parent?.addChild(token);
                    break;
            }
            // DebugConsole.Write("pushing parent with node type of " + parent.nodeType + " to parent stack and parent parent of node type " + parent?.parent?.nodeType);
            delimParentStack.Push(parent);
            delimLevel++;
        }
        else if (token.type == Util.TokenType.DelimiterClose)
        {
            AST.Node delimParent = delimParentStack.Pop();
            switch (parent?.nodeType)
            {
                case AST.Node.NodeType.ForLoop:
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
            if (delimLevel == 0)
            {
                parent = null;
            }
            else
            {
                // DebugConsole.Write("setting parent to delim parent of node type " + delimParent?.nodeType + " with parent of node type " + delimParent?.parent?.nodeType);
                parent = delimParent.parent;
            }
        }

        return (parent, delimLevel);
    }


    public static List<AST.Node> parse(List<Util.Token> _tokenList, Spectre.Console.ProgressTask task = null)
    {
        tokenList = _tokenList;

        DebugConsole.WriteAnsi("[red]tokens[/]");
        foreach (Util.Token token in tokenList)
        {
            if (token.type != Util.TokenType.String)
            {
                DebugConsole.Write(token.value + " " + token.type);
            }
        }
        DebugConsole.WriteAnsi("[red]end tokens[/]");

        if (task != null)
        {
            task.MaxValue = tokenList.Count();
        }

        parent = null;
        int delimLevel = 0;
        int prevLine = 0;
        int prevColumn = 0;

        currentTokenNum = 0;

        List<Util.TokenType> expectedTypes = new List<Util.TokenType>();

        finalTokenNum = tokenList.Count();

        bool singleLineComment = false;
        bool multiLineComment = false;

        DebugConsole.Write("first token: " + tokenList[currentTokenNum].value + " num: " + currentTokenNum);

        while (!isFinishedParsing)
        {
            if (parent?.isExpression == false)
            {
                lastMajorParentNode = parent;
            }

            if (task != null)
            {
                task.Increment(1);
            }

            isFinishedParsing = currentTokenNum == finalTokenNum;

            Util.Token token = tokenList[currentTokenNum];

            parent?.addCode(token);
            if (token.type != Util.TokenType.Space)
            {
                DebugConsole.Write($"token of value: {token.value} and type of {token.type} and parent of {parent?.nodeType} and delim level of {delimLevel}");
            }

            prevLine = token.line;
            prevColumn = token.column;

            AST.Node? previousNode = nodes.Count > 0 && currentTokenNum > 0 ? nodes.Last() : null;

            if (token.type == Util.TokenType.EOF)
            {
                break;
            }
            else if (token.type == Util.TokenType.EOL)
            {
                if (singleLineComment)
                {
                    singleLineComment = false;
                    currentTokenNum++;
                    continue;
                }

                if (delimLevel > 0)
                {
                    while (parent?.newLineReset == true)
                    {
                        parent = parent.parent;

                    }
                    currentTokenNum++;
                    continue;
                }

                if (parent?.nodeType != AST.Node.NodeType.Function && parent?.nodeType != AST.Node.NodeType.IfStatement && parent?.nodeType != AST.Node.NodeType.ElseStatement && parent?.nodeType != AST.Node.NodeType.ForLoop /* && tokenList[tokenIndex - 1].value != "{" */ && delimLevel == 0)
                {
                    currentTokenNum++;
                    parent = null;
                    continue;
                }
                else
                {
                    currentTokenNum++;
                    continue;
                }
            }

            if (token.value == Config.settings.general.comment.singleLine)
            {
                singleLineComment = true;
                currentTokenNum++;
                continue;
            }
            else if (token.value == Config.settings.general.comment.multiLineOpen)
            {
                multiLineComment = true;
                currentTokenNum++;
                continue;
            }
            else if (token.value == Config.settings.general.comment.multiLineClose)
            {
                multiLineComment = false;
                currentTokenNum++;
                continue;
            }

            if (singleLineComment || multiLineComment)
            {
                currentTokenNum++;
                continue;
            }

            if (expectedTypes != null)
            {
                checkToken(token, expectedTypes);
            }

            switch (token.type)
            {
                case Util.TokenType.Space:
                    parent?.addSpace(token);
                    break;
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
                    AST.BinaryExpression binExpr = new AST.BinaryExpression(token, previousNode, parent);
                    currentTokenNum++;
                    parent = binExpr;
                    continue;

                case Util.TokenType.Keyword:
                    (AST.Node keyParent, int keyDelimLevel) = parseKeyword(token, currentTokenNum, parent, delimLevel);
                    //0 is the keyword AST.Node, 1 is the next token, and 2 is the next token index
                    currentTokenNum++;
                    parent = keyParent;
                    delimLevel = keyDelimLevel;
                    continue;
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
                        currentTokenNum++;
                        parent = varAss;
                        continue;
                        // VariableReAssignment varReAss = new VariableReAssignment(token);
                        // return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, varReAss, delimLevel: delimLevel);
                    }
                    break;
                case Util.TokenType.String:
                    new AST.StringExpression(token, parent);
                    currentTokenNum++;
                    continue;
                case Util.TokenType.Modifier:
                    switch (token.value)
                    {
                        case "*":
                            parent = new AST.Dereference(token, parent);
                            currentTokenNum++;
                            continue;
                        case "&":
                            currentTokenNum++;
                            parent = new AST.Reference(token, parent);
                            continue;
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
                                    parent = parent.parent;
                                }
                                currentTokenNum++;
                                continue;
                            }

                            if (parent?.nodeType != AST.Node.NodeType.Function && parent?.nodeType != AST.Node.NodeType.IfStatement && parent?.nodeType != AST.Node.NodeType.ElseStatement && parent?.nodeType != AST.Node.NodeType.ForLoop /* && tokenList[tokenIndex - 1].value != "{" */ && delimLevel == 0)
                            {
                                currentTokenNum++;
                                parent = null;
                                continue;
                            }
                            else
                            {
                                currentTokenNum++;
                                continue;
                            }
                        }
                        else
                        {
                            throw ParserException.FactoryMethod("Illegal semi colon", "Remove the semi colon", token, parent);
                        }
                        break;
                    }
                    else
                    {
                        parent?.addChild(token);
                        currentTokenNum++;
                        continue;
                    }
            }

            if (token.isDelim)
            {
                (AST.Node delimParentRet, int delimReturnLevel) = parseDelim(token, currentTokenNum, parent, delimLevel);
                currentTokenNum++;
                DebugConsole.Write("delim returned parent of " + delimParentRet?.nodeType);
                parent = delimParentRet;
                delimLevel = delimReturnLevel;
                continue;
                // return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, delimParent, delimLevel: delimRet[1]);
                // if (parent != null)
                //     return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent);
            }
            currentTokenNum++;
        }

        DebugConsole.WriteAnsi("[green]parser debug below[/]");
        printAST(nodes);
        DebugConsole.WriteAnsi("[green]parser debug end[/]");

        return nodes;
    }
}

