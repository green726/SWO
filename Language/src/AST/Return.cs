namespace AST;

public class Return : Node
{
    public Expression expr;

    public Return(Util.Token token, AST.Node parent) : base(token)
    {
        this.nodeType = NodeType.Return;
        this.newLineReset = true;
        this.generator = new Generator.Return(this);

        if (parent == null)
        {
            throw ParserException.FactoryMethod("Parentless (functionless) return keyword", "Delete the return or fix a typo", token, typoSuspected: true);
        }
        this.parent = parent;
        this.parent.addChild(this);
    }

    public override void addChild(Node child)
    {
        if (!child.isExpression)
        {
            throw ParserException.FactoryMethod("A non-expression was passed to a return function", "Delete it or replace it with an expression (number, variable, string, etc)", child);
        }
        this.expr = (Expression)child;
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {
        throw ParserException.FactoryMethod("A non-expression was passed to a return function", "Delete it or replace it with an expression (number, variable, string, etc)", child, parent);
    }

    public override void addParent(Node parent)
    {
        base.addParent(parent);
    }
}
