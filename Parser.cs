// to decide where everything goes in the final AST: assign every node type a "importance/level" value and then loop through all the ASTNodes and assign all the nodes from the highest level to the topAST primaryChildren?
using System.Text;

public static class Parser
{
    public static List<ASTNode> nodes = new List<ASTNode>();
    public static List<Util.Token> tokenList;

    public static class topAST
    {
        public static List<ASTNode> primaryChildren = new List<ASTNode>();
    }


    public abstract class ASTNode
    {
        public List<ASTNode>? children;
        public ASTNode? parent;

        public ASTNode getParent()
        {
            return parent;
        }

        public void addChild(ASTNode child)
        {
            children.Add(child);
        }
    }

    public abstract class Expression : ASTNode
    {
        public ExprType NodeType;
        public enum ExprType
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Declare
        }
    }


    public class NumberExpression : Expression
    {

        public double value;

        public NumberExpression(Util.Token token)
        {
            this.value = Double.Parse(token.value);
        }
    }

    public class BinaryExpression : Expression
    {
        public ASTNode leftHand;
        public ASTNode rightHand;

        public BinaryExpression(Util.Token token, ASTNode previousNode, Util.Token nextToken, ASTNode? parent)
        {
            switch (token.value)
            {
                case "+":
                    this.NodeType = ExprType.Add;
                    break;
                case "-":
                    this.NodeType = ExprType.Subtract;
                    break;
                case "*":
                    this.NodeType = ExprType.Multiply;
                    break;
                case "/":
                    this.NodeType = ExprType.Divide;
                    break;
                default:
                    throw new ArgumentException("op " + token.value + " is not a valid operator");
            }
            this.parent = parent;

            this.leftHand = previousNode;
            this.rightHand = new NumberExpression(checkToken(nextToken, Util.tokenType.number));

        }
    }

    public static Util.Token checkToken(Util.Token token, Util.tokenType expectedType)
    {
        if (token.type != expectedType)
        {
            throw new ArgumentException($"expected type {expectedType} but got {token.type} at {token.line}:{token.column}");
        }
        return token;
    }

    public static string printAST()
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach (ASTNode node in nodes)
        {
            if (node.GetType() == typeof(NumberExpression))
            {
                stringBuilder.Append(node.value);

            }
            else if (node.GetType() == typeof(BinaryExpression))
            {
                stringBuilder.Append(node.NodeType);
            }
        }

        return nodes.ToString();
    }

    public static bool parseToken(Util.Token token, int tokenIndex, ASTNode? parent = null)
    {
        Console.WriteLine($"parse loop {tokenIndex}: {printAST()}");
        if (tokenIndex == tokenList.Count - 1)
        {
            return true;
        }

        switch (token.type)
        {
            case Util.tokenType.number:
                nodes.Add(new NumberExpression(token));
                break;
            case Util.tokenType._operator:
                BinaryExpression binExpr = new BinaryExpression(token, nodes.Last(), tokenList[tokenIndex + 1], parent);
                nodes.Add(binExpr);
                return parseToken(tokenList[tokenIndex + 1], tokenIndex + 1, binExpr);

        }
        return parseToken(tokenList[tokenIndex + 1], tokenIndex + 1);

    }

    public static void beginParse(List<Util.Token> _tokenList)
    {
        tokenList = _tokenList;
        parseToken(tokenList[0], 0);
        Console.WriteLine(nodes[1]);
    }

}
