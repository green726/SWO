
namespace AST;

public class WhileLoop : Node
{
    public BinaryExpression condition;
    public List<AST.Node> body;

    private int parseIteration = 0;


    public WhileLoop(Util.Token token, AST.Node parent) : base(token)
    {
        this.nodeType = NodeType.WhileLoop;
        this.generator = new Generator.WhileLoop(this);
        if (parent != null)
        {
            this.parent = parent;
            parent.addChild(this);
        }
        else
        {
            throw ParserException.FactoryMethod("An illegal parentless (top level) for loop was created", "Place the for loop within a function", this);
        }
        this.body = new List<Node>();
    }

    public override void addChild(Node child)
    {
        if (condition == null)
        {
            switch (parseIteration)
            {
                case 0:
                    if (child.nodeType != NodeType.VariableExpression)
                    {
                        throw ParserException.FactoryMethod($"For Loop expected variable but found a {child.nodeType}", $"Remove the {child.nodeType} and replace it with a variable", child);
                    }
                    break;
                case 1:
                    if (child.nodeType != NodeType.BinaryExpression)
                    {
                        throw ParserException.FactoryMethod($"For Loop expected binary expression but found a {child.nodeType}", $"Remove the {child.nodeType} and replace it with a binary expression", child);
                    }
                    this.condition = (AST.BinaryExpression)child;
                    break;
            }
            parseIteration++;
        }
        else
        {
            body.Add(child);
        }

        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {
        //TODO: implement this:
        // throw ParserException.FactoryMethod();
        base.addChild(child);
    }

}
