namespace AST;

public class ArrayExpression : Expression
{
    public Type containedType;
    public int length;

    public List<Node> values = new List<Node>();

    public ArrayExpression(Util.Token token, AST.Node? parent = null) : base(token)
    {
        //TODO: replace this with array delim from config
        if (token.value != "{")
        {
            throw ParserException.FactoryMethod("An illegal opening delimiter was used for an ArrayExpression", "Replace it with the proper delimiter", token);
        }
        if (parent != null)
        {
            if (this.parent.nodeType == NodeType.VariableAssignment)
            {
                VariableAssignment varAss = (VariableAssignment)parent;
                this.containedType = varAss.type;
            }
            else
            {

            }
        }
        this.parent?.addChild(this);

    }

    public override void addChild(Node child)
    {
        if (!child.isExpression)
        {
            throw ParserException.FactoryMethod("A non expression wa sillegaly added to an array", "Replace it with an exception", child);
        }
        else
        {
            values.Add(child);
        }
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {
        base.addChild(child);
    }
}
