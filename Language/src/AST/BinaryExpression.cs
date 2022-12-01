namespace AST;

using System.Linq;

public class BinaryExpression : Expression
{
    public AST.Expression leftHand;
    public AST.Expression rightHand;
    public BinaryOperator binOp;

    //constructor that takes in a left hand, an operator type, and a parent node
    public BinaryExpression(AST.Expression leftHand, Util.Token binOpTok, AST.Node parent) : base(leftHand, parent)
    {
        this.leftHand = leftHand;
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
        this.rightHand = new AST.Empty();
        this.binOp = new BinaryOperator(binOpTok);
        this.parent = parent;

        checkPrecedence();
        this.parent.addChild(this);
        this.generator = new Generator.BinaryExpression(this);
        this.nodeType = NodeType.BinaryExpression;
        this.leftHand.parent.removeChild(this.leftHand);
        this.leftHand.parent = this;
    }

    public BinaryExpression(AST.Expression leftHand, AST.Expression rightHand, string binOpStr, AST.Node parent) : base(leftHand, parent)
    {
        this.leftHand = leftHand;
        this.rightHand = new AST.Empty();
        this.binOp = new BinaryOperator(binOpStr);
        this.parent = parent;

        checkPrecedence();
        this.parent.addChild(this);
        this.generator = new Generator.BinaryExpression(this);
        this.nodeType = NodeType.BinaryExpression;
        this.leftHand.parent.removeChild(this.leftHand);
        this.leftHand.parent = this;
    }

    public void checkPrecedence()
    {
        if (this.rightHand.nodeType == NodeType.BinaryExpression)
        {
            BinaryExpression rightBinExpr = (BinaryExpression)this.rightHand;
            this.rightHand.parent = this;
            if (this.binOp >= rightBinExpr.binOp)
            {
                return;
            }
            else
            {
                //3 * 2 + 4 = 10 (but in reality 18)
                //4 + 2 * 3 = 10 (but really 10)

                //this will swap the left and right hands
                this.leftHand = rightBinExpr.leftHand;
                this.rightHand = leftHand;

                rightBinExpr.leftHand = rightBinExpr.rightHand;
                rightBinExpr.rightHand = rightBinExpr.leftHand;

                rightBinExpr.parent = this.parent;
                this.parent = rightBinExpr;
            }
        }
    }

    public override void addChild(Node child)
    {
        DebugConsole.Write("adding child of type: " + child.nodeType + " to binExpr");
        //throw error if right hand node type is not Empty
        if (this.rightHand.nodeType != NodeType.Empty && child.nodeType != NodeType.BinaryExpression)
        {
            throw new ParserException("Cannot add child to BinaryExpression with non-empty right hand");
        }
        if (!child.isExpression)
        {
            throw new ParserException("BinaryExpression can only have Expression children");
        }
        this.rightHand = (AST.Expression)child;
        checkPrecedence();
        this.parent.addChild(this);
        base.addChild(child);
    }

}
