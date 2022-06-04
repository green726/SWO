using System.Collections.Generic;

public class IfStatement : ASTNode
{
    public BinaryExpression expression = null;
    public List<ASTNode> body = new List<ASTNode>();
    private bool isBody = false;
    // private bool isStat = true;


    public IfStatement(Util.Token token, ASTNode? parent = null) : base(token)
    {
        this.nodeType = NodeType.IfStatement;
        parent?.addChild(this);
    }

    public override void addChild(ASTNode child)
    {
        if (expression == null)
        {
            Parser.checkNode(child, new NodeType[] { NodeType.BinaryExpression, NodeType.NumberExpression, NodeType.VariableExpression });
            if (child.nodeType == NodeType.BinaryExpression)
            {
                expression = (BinaryExpression)child;
            }
            else
            {
                base.addChild(child);
                return;
            }
        }
        else if (isBody == true)
        {
            body.Add(child);
        }
        else
        {
            throw new ParserException($"Attempted to add multiple conditions to if statement (currently unsupported)", child);
        }
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {
        // if (child.type == Util.TokenType.ParenDelimiterClose)
        // {
        //     isStat = false;
        // }
        if (child.type == Util.TokenType.BrackDelimiterOpen)
        {
            isBody = true;
        }
        else
        {
            throw new ParserException($"illegal child ({child.value}) added to if statement", child);
        }
        base.addChild(child);
    }
}
