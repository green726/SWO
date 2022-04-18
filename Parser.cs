// to decide where everything goes in the final AST: assign every node type a "importance/level" value and then loop through all the ASTNodes and assign all the nodes from the highest level to the topAST primaryChildren?
using System.Text;

public static class Parser
{
    public static List<ASTNode> nodes = new List<ASTNode>();
    public static List<Util.Token> tokenList;

    public static Util.TokenType[] binaryExpectedTokens = { Util.TokenType.number };
    public static ASTNode.NodeType[] binaryExpectedNodes = { ASTNode.NodeType.NumberExpression, ASTNode.NodeType.BinaryExpression };

    public static class topAST
    {
        public static List<ASTNode> primaryChildren = new List<ASTNode>();
    }


    public abstract class ASTNode
    {
        public List<ASTNode> children = new List<ASTNode>();
        public ASTNode? parent;

        public NodeType nodeType;

        public enum NodeType
        {
            NumberExpression,
            BinaryExpression
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
        public ASTNode rightHand;
        public OperatorType operatorType;

        public enum OperatorType
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }

        public BinaryExpression(Util.Token token, ASTNode previousNode, Util.Token nextToken, ASTNode? parent)
        {
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

            checkNode(previousNode, binaryExpectedNodes);

            this.leftHand = previousNode;

            if (this.leftHand.parent == null && this.leftHand.nodeType == ASTNode.NodeType.NumberExpression)
            {
                this.leftHand.addParent(this);
            }

            // this.rightHand = new NumberExpression(checkToken(nextToken, Util.tokenType.number), this);

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

        public override void addChild(ASTNode child)
        {
            this.children.Add(child);
            this.rightHand = child;
        }
    }

    public static void checkNode(ASTNode node, ASTNode.NodeType[] expectedTypes)
    {
        foreach (ASTNode.NodeType expectedNodeType in expectedTypes)
        {
            if (node.nodeType != expectedNodeType && expectedNodeType == expectedTypes.Last())
            {
                throw new ArgumentException($"expected type {expectedTypes.ToString()} but got {node.nodeType}");
            }
            else if (node.nodeType == expectedNodeType)
            {
                break;
            }
        }
    }

    public static void checkToken(Util.Token token, Util.TokenType[] expectedTypes)
    {
        foreach (Util.TokenType expectedTokenType in expectedTypes)
        {
            if (token.type != expectedTokenType && expectedTokenType == expectedTypes.Last())
            {
                throw new ArgumentException($"expected type {expectedTypes.ToString()} but got {token.type} at {token.line}:{token.column}");
            }
            else if (token.type == expectedTokenType)
            {
                break;
            }
        }
    }

    public static string printAST()
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach (ASTNode node in nodes)
        {
            stringBuilder.Append(node.nodeType);
            stringBuilder.Append("\n");
        }

        return stringBuilder.ToString();
    }

    public static bool parseToken(Util.Token token, int tokenIndex, ASTNode? parent = null, Util.TokenType[]? expectedTypes = null)
    {
        Console.WriteLine($"parse loop {tokenIndex}: {printAST()}");
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
            case Util.TokenType.number:
                new NumberExpression(token, parent);
                break;
            case Util.TokenType._operator:
                BinaryExpression binExpr = new BinaryExpression(token, nodes.Last(), tokenList[tokenIndex + 1], parent);
                return parseToken(tokenList[tokenIndex + 1], tokenIndex + 1, binExpr, binaryExpectedTokens);

        }
        return parseToken(tokenList[tokenIndex + 1], tokenIndex + 1);

    }

    public static void beginParse(List<Util.Token> _tokenList)
    {
        tokenList = _tokenList;
        parseToken(tokenList[0], 0);

        Console.WriteLine("node types below");
        Console.WriteLine(printAST());
    }

}
