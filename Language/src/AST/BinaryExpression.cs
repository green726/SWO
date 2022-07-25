namespace AST;

using System.Linq;

public class BinaryExpression : Expression
{
    public AST.Node leftHand;
    public AST.Node? rightHand;
    public OperatorType operatorType;

    public enum OperatorType
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Equals,
        LessThan,
    }

    public BinaryExpression(Util.Token token, AST.Node? previousNode, AST.Node? parent) : base(token)

    {
        this.nodeType = NodeType.BinaryExpression;
        this.generator = new Generator.BinaryExpression(this);

        this.newLineReset = true;

        //TODO: implement operator precedence parsing
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
            default:
                throw ParserException.FactoryMethod("An unknown binary operator was used", "use a known binary operator (such as \" == \" for comparison or \" + \" for addition)", token);
        }

        this.parent = parent;
        if (this.parent != null)
        {
            if (this.parent.nodeType == NodeType.Function)
            {
                Function prevFunc = (Function)parent;
                Parser.checkNode(prevFunc.body.Last(), Parser.binaryExpectedNodes);
                this.leftHand = prevFunc.body.Last();
            }
            else if (this.parent.nodeType == NodeType.FunctionCall)
            {
                FunctionCall prevCall = (FunctionCall)parent;
                Parser.checkNode(prevCall.args.Last(), Parser.binaryExpectedNodes);
                this.leftHand = prevCall.args.Last();
            }
            else if (this.parent.nodeType == NodeType.IfStatement)
            {
                IfStatement ifStat = (IfStatement)parent;
                this.leftHand = ifStat.children.Last();
                ifStat.children.RemoveAt(ifStat.children.Count - 1);
            }
            else if (this.parent.nodeType == NodeType.VariableAssignment)
            {
                VariableAssignment varAss = (VariableAssignment)parent;
                varAss.addChild(this);
                return;
            }
            else
            {
                Parser.checkNode(previousNode, Parser.binaryExpectedNodes);
                this.leftHand = previousNode;
            }
        }
        if (this?.leftHand?.nodeType == AST.Node.NodeType.NumberExpression || this?.leftHand?.nodeType == NodeType.VariableExpression)
        {
            this.leftHand.addParent(this);
        }
        else if (parent == null && this.leftHand.nodeType == NodeType.BinaryExpression)
        {
            this.parent = this.leftHand;
        }


        // this.rightHand = new NumberExpression(checkToken(nextToken, Util.tokenType.number), this);

        if (this.parent == null)
        {
            //NOTE: - commented out below code is to throw in an anonymous function 
            // PrototypeAST proto = new PrototypeAST();
            // FunctionAST func = new FunctionAST(proto, this);
            Parser.nodes.Add(this);
        }
        else
        {
            this.parent.addChild(this);
        }

    }
    public override void addChild(AST.Node child)
    {
        base.addChild(child);
        if (child.nodeType == AST.Node.NodeType.BinaryExpression)
        {
        }
        else if (rightHand == null)
        {
            this.rightHand = child;
        }
        else if (rightHand != null)
        {
            this.parent.addChild(child);
            child.addParent(this.parent);
        }
    }
}
