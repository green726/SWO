namespace AST;

public class VariableAssignment : AST.Node
{
    public VariableExpression varExpr;

    public Type type;
    public string assignmentOp = "";
    private int childLoop = 0;

    public bool binReassignment = false;
    public BinaryExpression? bin = null;
    public AST.Node? targetValue = null;

    public VariableAssignment(Util.Token token, AST.Node? parent = null) : base(token)
    {
        this.nodeType = NodeType.VariableAssignment;
        this.generator = new Generator.VariableAssignment(this);

        this.newLineReset = true;

        if (parent != null)
        {
            if (parent.nodeType == NodeType.VariableExpression)
            {
                VariableExpression prevVarExpr = (VariableExpression)parent;
                prevVarExpr.addParent(this);
                this.varExpr = prevVarExpr;
                // prevVarExpr.isPointer = true;
                this.children.Add(prevVarExpr);

                this.parent = Parser.lastMajorParentNode;
                this.parent?.addChild(this);
            }

            else
            {
                this.parent = parent;
                this.parent?.addChild(this);
            }


            this.childLoop = 1;
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


    public override void addChild(Util.Token child)
    {
        switch (childLoop)
        {
            case 0:
                break;
        }
        childLoop++;

    }

    public override void addChild(AST.Node node)
    {
        base.addChild(node);
        DebugConsole.Write("adding child of node type " + node.nodeType + " to varass");

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
                    this.targetValue = node;
                }
                break;
        }
        // this.targetValue = node;
        childLoop++;
    }


}
