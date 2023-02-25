namespace AST;
using Spectre.Console;

public class IndexReference : Expression
{
    public Expression expr;

    public IndexReference(Util.Token token, AST.Node parent) : base(token, parent)
    {
        DebugConsole.Write("in index ref");
        this.nodeType = NodeType.IndexReference;
        this.generator = new Generator.IndexReference(this);

        this.parent = parent;
        DebugConsole.Write($"IndexReference constructor called with parent {parent.nodeType}");
        this.parent?.addChild(this);

        if (parent.nodeType == AST.Node.NodeType.Empty)
        {
            throw new Exception();
        }

        if (!this.parent.isExpression)
        {
            throw new Exception();
        }
    }

    public override void addChild(AST.Node child)
    {
        if (child.isExpression && this.expr == null)
        {
            this.expr = (Expression)child;
            this.codeExcerpt += child.codeExcerpt;
        }
        else if (this.expr != null && child.isExpression) {
            this.children.Add(child);
            base.addChild(child);
        }
        else
        {
            throw ParserException.FactoryMethod($"Non-expression child added of type {child.nodeType} to IndexReference", "Remove it and replace it with an expression", child, this);
        }
    }
}
