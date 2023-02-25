
public class BinaryOperator
{
    public static Dictionary<OperatorType, int> operatorPrecedences = new Dictionary<OperatorType, int>() {
        {OperatorType.Add, 20},
        {OperatorType.Subtract, 20},
        {OperatorType.Multiply, 40},
        {OperatorType.Divide, 40},
        // {OperatorType.Modulo, 40},
        // {OperatorType.Equal, 10},
        // {OperatorType.NotEqual, 10},
        {OperatorType.GreaterThan, 10},
        // {OperatorType.GreaterThanOrEqual, 10},
        {OperatorType.LessThan, 10},
        // {OperatorType.LessThanOrEqual, 10},
        // {OperatorType.And, 5},
        // {OperatorType.Or, 5},
    };

    private bool parenEncapsulated = false;

    public int precedence
    {
        get
        {
            if (parenEncapsulated)
            {
                return 100;
            }
            else
            {
                return operatorPrecedences[operatorType];
            }
        }
    }

    //write a method to check if the operator's type is a comparison operator
    public bool isComparisonOperator()
    {
        return operatorType == OperatorType.Equals || /* operatorType == OperatorType.NotEqual || */ operatorType == OperatorType.GreaterThan || /* operatorType == OperatorType.GreaterThanOrEqual || */ operatorType == OperatorType.LessThan /* || operatorType == OperatorType.LessThanOrEqual */;
    }

    //write a method to reverse comparison operators
    public void reverseComparisonOperator()
    {
        switch (operatorType)
        {
            case OperatorType.Equals:
                operatorType = OperatorType.Equals;
                break;
            // case OperatorType.NotEqual:
            //     operatorType = OperatorType.NotEqual;
            //     break;
            case OperatorType.GreaterThan:
                operatorType = OperatorType.LessThan;
                break;
            // case OperatorType.GreaterThanOrEqual:
            //     operatorType = OperatorType.LessThanOrEqual;
            //     break;
            case OperatorType.LessThan:
                operatorType = OperatorType.GreaterThan;
                break;
            // case OperatorType.LessThanOrEqual:
            //     operatorType = OperatorType.GreaterThanOrEqual;
            //     break;
        }
    }

    public void parenEncapsulate()
    {
        this.parenEncapsulated = true;
    }

    //override the equals method
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        return this.operatorType == ((BinaryOperator)obj).operatorType;
    }

    //overload the > operator
    public static bool operator >(BinaryOperator op1, BinaryOperator op2)
    {
        return op1.precedence > op2.precedence;
    }

    //overload the < operator
    public static bool operator <(BinaryOperator op1, BinaryOperator op2)
    {
        return op1.precedence < op2.precedence;
    }

    //overload the == operator
    public static bool operator ==(BinaryOperator op1, BinaryOperator op2)
    {
        return op1.precedence == op2.precedence;
    }

    //overload the != operator
    public static bool operator !=(BinaryOperator op1, BinaryOperator op2)
    {
        return op1.precedence != op2.precedence;
    }

    //overload the >= operator and the <= operator
    public static bool operator >=(BinaryOperator op1, BinaryOperator op2)
    {
        return op1.precedence >= op2.precedence;
    }

    public static bool operator <=(BinaryOperator op1, BinaryOperator op2)
    {
        return op1.precedence <= op2.precedence;
    }

    public enum OperatorType
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Equals,
        LessThan,
        GreaterThan,
    }

    public OperatorType operatorType;

    //compare to method based on the operator precendence
    public int CompareTo(BinaryOperator other)
    {
        return this.precedence - other.precedence;
    }

    //override the get hash code method
    public override int GetHashCode()
    {
        return this.operatorType.GetHashCode();
    }

    //constuctor that takes in a token and checks its operator type based on its value
    public BinaryOperator(Util.Token token)
    {
        //compare the token.value to the operator types
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
            case "==":
                this.operatorType = OperatorType.Equals;
                break;
            case "<":
                this.operatorType = OperatorType.LessThan;
                break;
            case ">":
                this.operatorType = OperatorType.GreaterThan;
                break;
            default:
                throw ParserException.FactoryMethod("An unknown binary operator was used", "use a known binary operator (such as \" == \" for comparison or \" + \" for addition)", token);
        }
    }

    //a constructor that takes in a string and checks its operator type
    public BinaryOperator(string str)
    {
        switch (str)
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
            case "==":
                this.operatorType = OperatorType.Equals;
                break;
            case "<":
                this.operatorType = OperatorType.LessThan;
                break;
            case ">":
                this.operatorType = OperatorType.GreaterThan;
                break;
            default:
                throw ParserException.FactoryMethod("An unknown binary operator was used", "use a known binary operator (such as \" == \" for comparison or \" + \" for addition)", str);
        }
    }

}
