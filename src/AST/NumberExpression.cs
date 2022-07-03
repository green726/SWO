using System;

public class NumberExpression : ASTNode
{
    public double value;
    public TypeAST type;

    public NumberExpression(Util.Token token, ASTNode? parent) : base(token)

    {
        this.nodeType = NodeType.NumberExpression;
        switch (token.type)
        {
            case Util.TokenType.Double:
                this.value = Double.Parse(token.value);
                this.type = new TypeAST("double", this);
                break;
            case Util.TokenType.Int:
                this.value = Int64.Parse(token.value);
                this.type = new TypeAST("int", this);
                break;
        }
        this.parent = parent;

        if (parent != null)
        {
            this.parent.addChild(this);
        }
        else
        {
            Parser.nodes.Add(this);
        }
    }

}
