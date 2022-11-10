namespace AST;

public class ArrayExpression : Expression
{
    public int length;

    public ArrayExpression(Util.Token token, AST.Node parent) : base(token)
    {
        this.value = new List<AST.Node>();
        this.nodeType = NodeType.ArrayExpression;
        this.generator = new Generator.ArrayExpression(this);

        //TODO: replace this with array delim from config
        if (token.value != "{")
        {
            throw ParserException.FactoryMethod("An illegal opening delimiter was used for an ArrayExpression", "Replace it with the proper delimiter", token);
        }
        if (parent != null)
        {
            this.parent = parent;
            if (this.parent.nodeType == NodeType.VariableDeclaration)
            {
                VariableDeclaration varDec = (VariableDeclaration)parent;
            }
            else
            {

            }
        }
        this.parent?.addChild(this);

        //TODO: implement this
        // this.type = new Type();

    }

    public override void addChild(Node child)
    {
        if (!child.isExpression)
        {
            throw ParserException.FactoryMethod("A non expression was illegaly added to an array", "Replace it with an exception", child);
        }
        else
        {
            value.Add(child);
        }
        this.length++;
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {

        base.addChild(child);
    }
}
