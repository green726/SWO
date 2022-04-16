
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
        }
    }


    public static void Parse()
    {

    }
}
