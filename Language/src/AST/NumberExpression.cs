namespace AST;

using System;

public class NumberExpression : Expression
{
    // new public double value;
    public Type type;

    public NumberExpression(Util.Token token, AST.Node? parent) : base(token)

    {
        this.nodeType = NodeType.NumberExpression;
        this.generator = new Generator.NumberExpression(this);

        switch (token.type)
        {
            case Util.TokenType.Double:
                this.value = Double.Parse(token.value);
                this.type = new Type("double", this);
                break;
            case Util.TokenType.Int:
                this.value = Int64.Parse(token.value);
                this.type = new Type("int", this);
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
