
public static class Parser
{




    public static void Parse()
    {

    }

    public abstract class Expression
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
    /*
        //numerical expression
        public class NumExpr : Expression
        {
            public NumExpr(string value)
            {
                this.value = int.Parse(value);
                this.exprType = ExprType.declare;
            }

        }
    */
    public class BinaryExpression : Expression
    {
        public BinaryExpression(Lexer.Token token, Lexer.Token previousToken, Lexer.Token nextToken)
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
        }
    }


}
