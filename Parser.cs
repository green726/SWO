
public static class Parser
{

    public class AST
    {
        public List<ASTNode> nodes;



    }

    public abstract class ASTNode
    {
        public List<ASTNode> children;
        public ASTNode parent;

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

        public BinaryExpression(Util.Token token, Util.Token previousToken, Util.Token nextToken, ASTNode parent)
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

            this.leftHand = checkToken(previousToken, Util.tokenType.number);
            this.rightHand = checkToken(nextToken, Util.tokenType.number);

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

    public static void parseProgram()
    {

    }

}
