namespace AST;

public class Cast : Expression
{
    public TypeInformation desiredType;
    public Expression target;

    public Cast(TypeInformation desired, AST.Node target, AST.Node parent) : base(target)
    {
        this.nodeType = NodeType.Cast;
        this.generator = new Generator.Cast(this);
        this.parent = parent;
        this.parent.addChild(this);
        this.newLineReset = true;

        this.value = desired.value;
        this.desiredType = desired;
        this.target = (Expression)target;
    }

    public Cast(Util.Token token, AST.Node target, AST.Node parent) : base(token)
    {
        this.nodeType = NodeType.Cast;
        this.generator = new Generator.Cast(this);
        this.parent = parent;
        this.newLineReset = true;

        this.value = token.value;
        this.desiredType = new ParserTypeInformation(token.value);
        this.target = (Expression)target;
    }

    public Cast(Util.Token token, AST.Node parent) : base(token)
    {
        this.nodeType = NodeType.Cast;
        this.generator = new Generator.Cast(this);
        this.parent = parent;
        this.newLineReset = true;

        this.value = token.value;
        this.desiredType = new ParserTypeInformation(token.value);
    }

    public override void addChild(Node child)
    {
        base.addChild(child);

        if (!child.isExpression)
        {
            throw ParserException.FactoryMethod("Target value for a cast must be an expression", "Replace the non-expression target value with an expression", child, this);
        }
        this.target = (Expression)child;
        this.target.parent.removeChild(this.target);
        this.target.parent = this;
    }
}
