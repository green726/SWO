namespace AST;

public class VariableAssignment : AST.Node
{
    public VariableExpression varExpr;

    public Type type;
    public string assignmentOp = "";
    private int childLoop = 0;

    public bool binReassignment = false;
    public BinaryExpression bin;
    public AST.Node targetValue;

    public VariableAssignment(Util.Token token, AST.Node parent) : base(token)
    {
        DebugConsole.Write("variable assignment top prev expr: " + parser.previousExpression);
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

                this.parent = parser.lastMajorParentNode;
                DebugConsole.Write("variable assignment last major parent node: " + this.parent.nodeType);
                this.parent?.addChild(this);
            }
            else
            {
                DebugConsole.Write("prev expression of parser: " + parser.previousExpression);
                VariableExpression prevVarExpr = (VariableExpression)parser.previousExpression;
                prevVarExpr.addParent(this);
                this.varExpr = prevVarExpr;

                this.children.Add(prevVarExpr);
                this.parent = parent;
                this.parent?.addChild(this);
            }

            this.childLoop = 0;
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

        if (token.value.Substring(1) == "=" && parser.binaryMathOps.Contains(token.value.Remove(1)))
        {
            DebugConsole.WriteAnsi("[yellow]compound var ass detected[/]");
            string tokenOp = token.value.Remove(1);

            this.binReassignment = true;
            this.bin = new BinaryExpression(varExpr, new Util.Token(Util.TokenType.Operator, tokenOp, token.line, token.column), this);
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
        DebugConsole.Write("adding child of node type " + node.nodeType + " to varass with varExpr of " + varExpr.value + " with loop iteration of: " + childLoop + " and binReassignment of " + this.binReassignment);
        if (this.binReassignment == true)
        {
            // if(node == this.bin)
            if (node.nodeType == NodeType.BinaryExpression)
            {
                DebugConsole.Write("not adding child");
                return;
            }
            this.bin.addChild(node);
            DebugConsole.WriteAnsi("[yellow]adding child to varass bin[/]");
            childLoop++;
            return;
        }
        switch (childLoop)
        {
            case 0:
                if (node.nodeType == NodeType.BinaryExpression)
                {
                    BinaryExpression binExpr = (BinaryExpression)node;
                    binExpr.leftHand = (AST.Expression)this.children.Last();
                    this.binReassignment = true;
                    this.bin = binExpr;
                }
                else
                {
                    this.binReassignment = false;
                    this.targetValue = node;
                }
                break;
            case 1:
                break;
        }
        // this.targetValue = node;
        childLoop++;
        base.addChild(node);
    }
}
