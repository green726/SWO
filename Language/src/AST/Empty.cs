namespace AST;

public class Empty : Expression
{
    public Empty() : base(true)
    {
        this.nodeType = NodeType.Empty;
    }

    public override void addChild(Node child)
    {
        throw ParserException.FactoryMethod("An empty node was referenced", "This is an internal compiler error - make an issue on GitHub", this);
    }

    public override void addChild(Util.Token child)
    {
        throw ParserException.FactoryMethod("An empty node was referenced", "This is an internal compiler error - make an issue on GitHub", this);
    }

    public override void addCode(Util.Token code)
    {
        throw ParserException.FactoryMethod("An empty node was referenced", "This is an internal compiler error - make an issue on GitHub", this);
    }

    public override void addCode(string code)
    {
        throw ParserException.FactoryMethod("An empty node was referenced", "This is an internal compiler error - make an issue on GitHub", this);
    }

    public override void addNL()
    {
        throw ParserException.FactoryMethod("An empty node was referenced", "This is an internal compiler error - make an issue on GitHub", this);
    }

    public override void addParent(Node parent)
    {
        throw ParserException.FactoryMethod("An empty node was referenced", "This is an internal compiler error - make an issue on GitHub", this);
    }

    public override void addSpace(Util.Token space)
    {
        throw ParserException.FactoryMethod("An empty node was referenced", "This is an internal compiler error - make an issue on GitHub", this);
    }

    public override void checkExport()
    {
        throw ParserException.FactoryMethod("An empty node was referenced", "This is an internal compiler error - make an issue on GitHub", this);
    }

    public override void removeChild(Node child)
    {
        throw ParserException.FactoryMethod("An empty node was referenced", "This is an internal compiler error - make an issue on GitHub", this);
    }
}
