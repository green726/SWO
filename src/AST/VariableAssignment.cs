namespace AST;

public class VariableAssignment : AST.Node
{
    public string name = "";
    public Type type;
    public string assignmentOp = "";
    public string strValue = "";
    public bool mutable = false;
    private int childLoop = 0;

    public bool reassignment = false;
    public bool binReassignment = false;
    public BinaryExpression? bin = null;
    public AST.Node targetValue = null;

    public VariableAssignment(Util.Token token, bool mutable, AST.Node? parent = null) : base(token)
    {
        this.newLineReset = true;

        this.mutable = mutable;
        this.nodeType = NodeType.VariableAssignment;
        Parser.globalVarAss.Add(this);

        if (token.value != "const" && token.value != "var")
        {
            reassignment = true;
            if (parent != null)
            {

                AST.Node prevNode = parent.children.Last();
                if (prevNode.nodeType == NodeType.VariableExpression)
                {
                    VariableExpression prevVarExpr = (VariableExpression)prevNode;
                    this.name = prevVarExpr.varName;
                    prevVarExpr.addParent(this);
                    this.children.Add(prevVarExpr);
                }
                this.parent = parent;
                this.parent.addChild(this);
            }
            else
            {
                throw new ParserException("illegal top level variable reassignment", token);
                // AST.Node prevNode = Parser.nodes.Last();
                // if (prevNode.nodeType == AST.Node.NodeType.VariableExpression)
                // {
                //     VariableExpression prevVarExpr = (VariableExpression)prevNode;
                //     this.name = prevVarExpr.varName;
                //     prevVarExpr.addParent(this);
                //     this.children.Add(prevVarExpr);
                // }
            }
        }
        else
        {

            Parser.nodes.Add(this);
        }
    }

    public override void addChild(Util.Token child)
    {
        if (!reassignment)
        {
            switch (childLoop)
            {
                case 0:
                    this.type = new Type(child);
                    break;
                case 1:
                    this.name = child.value;
                    Typo.addToLittle(this);
                    break;
                case 2:
                    if (child.type != Util.TokenType.AssignmentOp) throw new ParserException($"expected assignment op but got {child.type} in a variable assignment", child);
                    this.assignmentOp = child.value;
                    break;
                case 3:
                    this.strValue = child.value;
                    break;
                default:
                    throw new ParserException($"Illegal extra items after variable assignment", this);

            }
        }
        else
        {
            switch (childLoop)
            {
                case 0:
                    break;
            }
        }
        childLoop++;


    }

    public override void addChild(AST.Node node)
    {
        base.addChild(node);
        Console.WriteLine("adding child of node type " + node.nodeType + "to varass");
        if (!reassignment)
        {
            switch (node.nodeType)
            {
                case NodeType.StringExpression:
                    if (childLoop == 3)
                    {
                        StringExpression strExp = (StringExpression)node;
                        this.strValue = strExp.value;
                    }
                    else
                    {
                        throw new ParserException($"Illegal value (type {node.nodeType}) of variable {this.name}", node);
                    }
                    break;
            }
        }
        else
        {
            switch (childLoop)
            {
                case 1:
                    if (node.nodeType == NodeType.BinaryExpression)
                    {
                        BinaryExpression binExpr = (BinaryExpression)node;
                        binExpr.leftHand = this.children.Last();
                        this.binReassignment = true;
                        this.bin = binExpr;
                    }
                    else
                    {
                        this.binReassignment = false;
                        this.targetValue = this.children.Last();
                    }
                    break;
            }
            this.targetValue = node;
            childLoop++;
        }
    }


}
