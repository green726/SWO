namespace AST;

using System;

public class NumberExpression : Expression
{
    public NumberExpression(Util.Token token, AST.Node parent) : base(token)

    {
        this.nodeType = NodeType.NumberExpression;
        this.generator = new Generator.NumberExpression(this);

        switch (token.type)
        {
            case Util.TokenType.Double:
                this.value = (double)Double.Parse(token.value);
                this.type = new ParserTypeInformation("double");
                break;
            case Util.TokenType.Int:
                this.value = (int)int.Parse(token.value);
                this.type = new ParserTypeInformation("int");
                break;
        }
        this.parent = parent;

        if (parent != null)
        {
            this.parent.addChild(this);
        }
        else
        {
            parser.nodes.Add(this);
        }
    }
}
