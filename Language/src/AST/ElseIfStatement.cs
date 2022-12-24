namespace AST;


public class ElseIfStatement : Node
{
    public ElseIfStatementConditional conditional;
    public List<Node> body = new List<Node>();

    private bool isBody = false;
    private int tokenChildIdx = 0;

    public ElseIfStatement(Util.Token token, AST.Node parent) : base(token, parent)
    {
        this.nodeType = NodeType.ElseIfStatement;
        this.generator = new Generator.ElseIfStatement(this);
        this.conditional = new ElseIfStatementConditional(token, this);
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

public class ElseIfStatementConditional : Node
{
    public Expression condition { get; set; } = new AST.Empty();

    private int tokenChildIdx = 0;

    public ElseIfStatementConditional(Util.Token token, AST.Node parent) : base(token, parent)
    {
        this.nodeType = NodeType.ElseIfStatementConditional;
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
                    throw ParserException.FactoryMethod("Illegal child added to else if statement", "Remove it and replace it with the legal delimiter, \"(\"", child, this);
                }
                break;
            case 1:
                if (child.value != ")")
                {
                    throw ParserException.FactoryMethod("Illegal child added to else if statement", "Remove it and replace it with the legal delimiter, \")\"", child, this);
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
                throw ParserException.FactoryMethod("A non expression was used as a conditional in an else if statement", "Remove it and replace it with an expression (ie foo == bar) or (funcThatReturnsBool())", child, this);
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
