namespace AST;

using System.Linq;

public class BinaryExpression : Expression
{
    public AST.Expression leftHand;
    public AST.Expression rightHand;
    public BinaryOperator binOp;

    //NOTE: maybe rewrite the binary expression to store other binary expressions in the left hand instead of the right hand?

    //constructor that takes in a left hand, an operator type, and a parent node
    public BinaryExpression(AST.Expression leftHand, Util.Token binOpTok, AST.Node parent) : base(leftHand, parent)
    {
        this.leftHand = leftHand;
        DebugConsole.Write("binary expression with left hand of type: " + leftHand.nodeType + " and value of: " + leftHand.value + " that has parent of type: " + leftHand.parent.nodeType);
        this.rightHand = new AST.Empty();
        this.binOp = new BinaryOperator(binOpTok);
        this.parent = parent;
        this.generator = new Generator.BinaryExpression(this);
        this.nodeType = NodeType.BinaryExpression;
        this.leftHand.parent.removeChild(this.leftHand);
        this.leftHand.parent = this;
    }

    public BinaryExpression(AST.Expression leftHand, AST.Expression rightHand, Util.Token binOpTok, AST.Node parent) : base(leftHand, parent)
    {
        this.leftHand = leftHand;
        DebugConsole.Write("binary expression with left hand of type: " + leftHand.nodeType + " and value of: " + leftHand.value + " that has parent of type: " + leftHand.parent.nodeType);
        this.rightHand = new AST.Empty();
        this.rightHand = new AST.Empty();
        this.binOp = new BinaryOperator(binOpTok);
        this.parent = parent;
        checkPrecedence();
        this.generator = new Generator.BinaryExpression(this);
        this.nodeType = NodeType.BinaryExpression;
        this.leftHand.parent.removeChild(this.leftHand);
        this.leftHand.parent = this;
        this.parent.addChild(this);
    }

    public BinaryExpression(AST.Expression leftHand, AST.Expression rightHand, string binOpStr, AST.Node parent) : base(leftHand, parent)
    {
        this.leftHand = leftHand;
        DebugConsole.Write("binary expression with left hand of type: " + leftHand.nodeType + " and value of: " + leftHand.value + " that has parent of type: " + leftHand.parent.nodeType);
        this.rightHand = new AST.Empty();
        this.rightHand = new AST.Empty();
        this.binOp = new BinaryOperator(binOpStr);
        this.parent = parent;

        checkPrecedence();
        this.generator = new Generator.BinaryExpression(this);
        this.nodeType = NodeType.BinaryExpression;
        this.leftHand.parent.removeChild(this.leftHand);
        this.leftHand.parent = this;
        this.parent.addChild(this);
    }

    public void checkPrecedence()
    {
        if (this.parent.nodeType == NodeType.ParenEncapsulation)
        {
            this.binOp.parenEncapsulate();
            DebugConsole.WriteAnsi("[red]detected paren encapsulated binexpr[/]");
        }
        if (this.rightHand.nodeType == NodeType.BinaryExpression)
        {
            BinaryExpression rightBinExpr = (BinaryExpression)this.rightHand;
            this.rightHand.parent = this;

            //this is less than b/c SWO is weird and evaluates the binary operators from right to left, so the higher precedence operators should be furthest right
            if (this.binOp <= rightBinExpr.binOp)
            {
                return;
            }
            else
            {
                DebugConsole.WriteAnsi("[purple]before swapped ops below:[/]");
                DebugConsole.Write($"original left hand:\n  leftHand {this.leftHand} | rightHand {this.rightHand}");
                DebugConsole.Write($"original right hand:\n  leftHand {rightBinExpr.leftHand} | rightHand {rightBinExpr.rightHand}");
                DebugConsole.Write($"original left hand parent:\n {this.parent}");
                DebugConsole.Write($"original right hand parent:\n {rightBinExpr.parent}");

                //4 * 4 < 10
                //4 * 4 < 10
                //10 > 4 * 4

                //3 * 2 + 4 = 10 (but in reality 18)
                //4 + 2 * 3 = 10 (but really 10)

                if (rightBinExpr.binOp.isComparisonOperator()) {
                    rightBinExpr.binOp.reverseComparisonOperator();
                }

                //this will swap the left and right hands
                this.rightHand = this.leftHand;
                this.leftHand = rightBinExpr.leftHand;

                rightBinExpr.leftHand = rightBinExpr.rightHand;
                rightBinExpr.rightHand = this;

                this.parent.removeChild(this);
                this.parent.addChild(rightBinExpr);
                rightBinExpr.parent = this.parent;
                this.parent = rightBinExpr;

                //1 + 2 * 3
                DebugConsole.WriteAnsi("[purple]precedence swapped ops below:[/]");
                DebugConsole.Write($"original left hand:\n  leftHand {this.leftHand} | rightHand {this.rightHand}");
                DebugConsole.Write($"original right hand:\n  leftHand {rightBinExpr.leftHand} | rightHand {rightBinExpr.rightHand}");
                DebugConsole.Write($"original left hand parent:\n {this.parent}");
                DebugConsole.Write($"original right hand parent:\n {rightBinExpr.parent}");
            }
        }

        if (this.leftHand.type.value != this.rightHand.type.value)
        {
            throw ParserException.FactoryMethod($"Binary expression has mis-matched  left ({this.leftHand.type.value}) right ({this.rightHand.type.value}) hand types.", "Ensure that the right and left hand expressions have matching types.", this);
        }

        DebugConsole.Write("Setting bin expr type to type of left hand");
        this.type = this.leftHand.type;
    }

    public override void addChild(Node child)
    {
        DebugConsole.Write("adding child of type: " + child.nodeType + " to binExpr");
        //throw error if right hand node type is not Empty and child is not binexpr
        if (this.rightHand.nodeType != NodeType.Empty && child.nodeType != NodeType.BinaryExpression)
        {
            throw new ParserException("Cannot add child to BinaryExpression with non-empty right hand");
        }
        if (!child.isExpression)
        {
            throw new ParserException("BinaryExpression can only have Expression children");
        }
        AST.Expression prevRightHand = this.rightHand;
        this.rightHand = (AST.Expression)child;
        if (prevRightHand.nodeType == NodeType.Empty)
        {
            DebugConsole.Write("binary expression adding itself as a child to its parent (type " + this.parent.nodeType + ")");
            this.parent.addChild(this);
        }
        checkPrecedence();
        base.addChild(child);
    }
}
