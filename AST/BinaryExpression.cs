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
            Parser.checkNode(prevFunc.body.Last(), Parser.binaryExpectedNodes);
            this.leftHand = prevFunc.body.Last();
        }
        else
        {
            Parser.checkNode(previousNode, Parser.binaryExpectedNodes);
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
            Parser.nodes.Add(func);
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
