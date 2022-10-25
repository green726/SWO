namespace Generator;

public class Empty : Base
{
    public AST.Node node;
    public bool isEmpty = false;
    public Empty(AST.Node node)
    {
        this.node = node;
    }

    public override void generate()
    {
        throw GenException.FactoryMethod("An empty generator was called", "This is an internal compiler error - make an issue on GitHub", node);
    }
}
