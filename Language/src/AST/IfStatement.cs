namespace AST;


public class IfStatement : AST.Node
{
    public AST.Node followingBlock { get; set; } = new AST.Empty();

    public List<AST.Node> body { get; set; } = new List<AST.Node>();

    public IfStatementConditional conditional { get; set; }

    private int tokenChildIdx = 0;
    private bool isBody = false;

    public IfStatement(Util.Token token, AST.Node parent) : base(token, parent)
    {
        this.nodeType = NodeType.IfStatement;
        this.generator = new Generator.IfStatement(this);

        this.newLineReset = false;

        this.conditional = new IfStatementConditional(token, this);
    }

    public override void addChild(Util.Token child)
    {
        switch (tokenChildIdx)
        {
            case 0:
                if (child.value != "{")
                {
                    throw ParserException.FactoryMethod("Illegal child added to if statement", "Remove it and replace it with the legal delimiter, \"{\"", child, this);
                }
                this.isBody = true;
                break;
            case 1:
                if (child.value != "}")
                {
                    throw ParserException.FactoryMethod("Illegal child added to if statement", "Remove it and replace it with the legal delimiter, \"}\"", child, this);
                }
                break;
        }
        tokenChildIdx++;
        base.addChild(child);
    }

    public override void addChild(Node child)
    {
        if (this.isBody)
        {
            this.body.Add(child);
        }
        base.addChild(child);
    }
}

public class IfStatementConditional : Node
{
    public Expression condition { get; set; } = new AST.Empty();

    private int tokenChildIdx = 0;

    public IfStatementConditional(Util.Token token, AST.Node parent) : base(token, parent)
    {
        this.nodeType = NodeType.IfStatementConditional;
        // this.generator = new Generator.IfStatementConditional(this);
        this.parent = parent;
    }

    public override void addChild(Util.Token child)
    {
        switch (tokenChildIdx)
        {
            case 0:
                if (child.value != "(")
                {
                    throw ParserException.FactoryMethod("Illegal child added to if statement", "Remove it and replace it with the legal delimiter, \"(\"", child, this);
                }
                break;
            case 1:
                if (child.value != ")")
                {
                    throw ParserException.FactoryMethod("Illegal child added to if statement", "Remove it and replace it with the legal delimiter, \")\"", child, this);
                }
                break;
        }
        tokenChildIdx++;
        base.addChild(child);
    }

    public override void removeChild(Node child)
    {
        this.condition = new AST.Empty();
        base.removeChild(child);
    }

    public override void addChild(Node child)
    {
        if (this.condition.nodeType == NodeType.Empty)
        {
            if (!child.isExpression)
            {
                throw ParserException.FactoryMethod("A non expression was used as a conditional in an if statement", "Remove it and replace it with an expression (ie foo == bar) or (funcThatReturnsBool())", child, this);
            }
            this.condition = (AST.Expression)child;
        }
        // else if (this.condition.nodeType == NodeType.Empty)
        // {
        //     //throw ParserException.FactoryMethod about not being able to have a conditonless if statement
        //     throw ParserException.FactoryMethod("A conditionless if statement was found", "Remove it and replace it with a conditional", child, this);
        // }
        base.addChild(child);
    }
}
