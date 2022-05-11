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

    public abstract class ASTNode
    {
        public List<ASTNode> children = new List<ASTNode>();
        public ASTNode? parent;

        public int line = 0;
        public int column = 0;

        public NodeType nodeType;

        public enum NodeType
        {
            NumberExpression,
            BinaryExpression,
            Prototype,
            Function,
            FunctionCall,
            BuiltinCall
        }

        public virtual void addParent(ASTNode parent)
        {
            this.parent = parent;
            if (this.parent != null)
            {
                nodes.Remove(this);
            }
        }

        public virtual void addChild(ASTNode child)
        {
            children.Add(child);
        }
    }

    public class PrototypeAST : ASTNode
    {
        public string name;
        public Dictionary<Util.ClassType, string> arguments = new Dictionary<Util.ClassType, string>();

        public PrototypeAST(int line, int column, string name = "", List<Util.Token> arguments = null)
        {
            this.nodeType = NodeType.Prototype;
            this.name = name;

            bool typePredicted = true;
            Util.ClassType prevType = Util.ClassType.Double;
            if (arguments != null)
            {
                foreach (Util.Token item in arguments)
                {
                    // Console.WriteLine("funcArgs " + item.value);
                    if (typePredicted)
                    {

                        checkToken(item, expectedType: Util.TokenType.Keyword);
                        switch (item.value)
                        {
                            case "double":
                                prevType = Util.ClassType.Double;
                                break;
                            case "int":
                                prevType = Util.ClassType.Int;
                                break;
                            case "string":
                                prevType = Util.ClassType.String;
                                break;
                            default:
                                throw new ArgumentException($"expected type declaration but got something else at {item.line}:{item.column}");
                        }
                    }
                    else
                    {
                        this.arguments.Add(prevType, item.value);
                    }

                    //swap typePredicted
                    typePredicted = !typePredicted;
                }

            }
            else
            {
                this.arguments = new Dictionary<Util.ClassType, string>();
            }
        }
    }


    public class FunctionAST : ASTNode
    {
        public PrototypeAST prototype;
        public List<ASTNode> body;


        public FunctionAST(PrototypeAST prototype, List<ASTNode>? body = default(List<ASTNode>))
        {
            this.nodeType = NodeType.Function;
            this.prototype = prototype;
            this.body = body;
        }

        public FunctionAST(PrototypeAST prototype, ASTNode body)
        {
            this.nodeType = NodeType.Function;
            this.prototype = prototype;
            this.body = new List<ASTNode>();
            this.body.Add(body);
        }

    }

    public class FunctionCall : ASTNode
    {
        public string? functionName;
        public bool builtIn = false;
        public List<ASTNode> args;

        public FunctionCall(Util.Token token, List<ASTNode>? args, bool? builtInExpected = false)
        {
            this.nodeType = NodeType.FunctionCall;

            if (Util.builtinFuncs.Contains(token.value))
            {
                this.builtIn = true;
            }
            if (builtInExpected == true && this.builtIn == false)
            {
                throw new ArgumentException("builtin function expected but name does not exist");
            }

            this.functionName = token.value;
            this.args = args ??= new List<ASTNode>();

            nodes.Add(this);

        }

        public override void addChild(ASTNode child)
        {
            args.Add(child);
        }
    }


    public class NumberExpression : ASTNode
    {
        public double value;

        public NumberExpression(Util.Token token, ASTNode? parent)
        {
            this.value = Double.Parse(token.value);
            this.parent = parent;

            if (parent != null)
            {
                this.parent.addChild(this);
            }
            else
            {
                nodes.Add(this);
            }
        }

    }


    public class BinaryExpression : ASTNode
    {
        public ASTNode leftHand;
        public ASTNode? rightHand;
        public OperatorType operatorType;

        public enum OperatorType
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }

        public BinaryExpression(Util.Token token, ASTNode? previousNode, Util.Token nextToken, ASTNode? parent)
        {
            this.line = token.line;
            this.column = token.column;

            //TODO: implement operator precedence parsing
            this.nodeType = NodeType.BinaryExpression;
            switch (token.value)
            {
                case "+":
                    this.operatorType = OperatorType.Add;
                    break;
                case "-":
                    this.operatorType = OperatorType.Subtract;
                    break;
                case "*":
                    this.operatorType = OperatorType.Multiply;
                    break;
                case "/":
                    this.operatorType = OperatorType.Divide;
                    break;
                default:
                    throw new ArgumentException("op " + token.value + " is not a valid operator");
            }

            this.parent = parent;
            if (previousNode.nodeType == NodeType.Function)
            {
                FunctionAST prevFunc = (FunctionAST)previousNode;
                checkNode(prevFunc.body.Last(), binaryExpectedNodes);
                this.leftHand = prevFunc.body.Last();
            }
            else
            {
                checkNode(previousNode, binaryExpectedNodes);
                this.leftHand = previousNode;
            }
            if (this.leftHand.parent == null && this.leftHand.nodeType == ASTNode.NodeType.NumberExpression)
            {
                this.leftHand.addParent(this);
            }
            else if (parent == null && this.leftHand.nodeType == NodeType.BinaryExpression)
            {
                this.parent = this.leftHand;
            }


            // this.rightHand = new NumberExpression(checkToken(nextToken, Util.tokenType.number), this);


            if (this.parent != null)
            {
                this.parent.addChild(this);
            }
            else
            {
                //TODO: add the creation of an anonymous function for the binary expression here
                PrototypeAST proto = new PrototypeAST(this.line, this.column);
                FunctionAST func = new FunctionAST(proto, this);
                nodes.Add(func);
            }
        }

        public override void addChild(ASTNode child)
        {
            this.children.Add(child);
            if (child.nodeType == ASTNode.NodeType.BinaryExpression)
            {
            }
            else
            {
                this.rightHand = child;
            }
        }
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

    public static void printBinary(BinaryExpression bin)
    {
        Console.WriteLine($"{bin.nodeType} op: {bin.operatorType} lhs type: {bin.leftHand.nodeType} rhs type: {bin.rightHand.nodeType} binop children below:");
        printAST(bin.children);
    }

    public static void printFunc(FunctionAST func)
    {
        StringBuilder stringBuilder = new StringBuilder();
        Console.WriteLine($"{func.nodeType} name: {func.prototype.name} args: {Serialize(func.prototype.arguments.ToList())} body start: ");

        printAST(func.body);

        Console.WriteLine("function body end");
    }

    public static void printFuncCall(FunctionCall funcCall)
    {
        Console.WriteLine($"{funcCall.nodeType} with name of {funcCall.functionName} and args of {String.Join(", ", funcCall.args)}");
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
                    printBinary(bin);
                    break;
                case ASTNode.NodeType.Function:
                    FunctionAST func = (FunctionAST)node;
                    printFunc(func);
                    break;
                case ASTNode.NodeType.FunctionCall:
                    FunctionCall funcCall = (FunctionCall)node;
                    printFuncCall(funcCall);
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

    public static ASTNode parseKeyword(Util.Token token, int tokenIndex, ASTNode? parent = null)
    {
        List<dynamic> ret = new List<dynamic>();
        Util.Token nextToken = tokenList[tokenIndex + 1];
        int nextTokenIndex = tokenIndex + 1;

        switch (nextToken.type)
        {
            case Util.TokenType.SquareDelimiterOpen:
                break;
            case Util.TokenType.ParenDelimiterOpen:
                //treat it as a function call
                //token would be the name, next token would be delim, so we grab all tokens starting from the one after that until final delim
                FunctionCall funcCall = new FunctionCall(token, null, true);
                return funcCall;
        }
        return null;

    }

    public static bool parseTokenRecursive(Util.Token token, int tokenIndex, ASTNode? parent = null, Util.TokenType[]? expectedTypes = null)
    {

        ASTNode? previousNode = nodes.Count > 0 ? nodes.Last() : null;

        // Console.WriteLine($"parse loop {tokenIndex}: {printAST()}");
        if (token.type == Util.TokenType.EOF)
        {
            return true;
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
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, binExpr, binaryExpectedTokens);

            case Util.TokenType.Keyword:
                ASTNode keyword = parseKeyword(token, tokenIndex, parent);
                //0 is the keyword ASTNode, 1 is the next token, and 2 is the next token index
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, keyword);
        }

        if (token.isDelim)
        {
            if (parent != null)
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent);
        }
        return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1);

    }


    public static List<ASTNode> beginParse(List<Util.Token> _tokenList)
    {
        tokenList = _tokenList;
        parseTokenRecursive(tokenList[0], 0);

        Console.WriteLine("BEGIN OF PARSER DEBUG");
        printAST(nodes);
        Console.WriteLine("END OF PARSER DEBUG");

        return nodes;
    }

}
