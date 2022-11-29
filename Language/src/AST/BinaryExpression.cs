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
        if (!child.isExpression)
        {
            throw new ParserException("BinaryExpression can only have Expression children");
        }
        checkPrecedence();
        this.parent.addChild(this);
        base.addChild(child);
    }

}
