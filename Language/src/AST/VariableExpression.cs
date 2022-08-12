namespace AST;

public class VariableExpression : Expression
{

    public bool isArrayIndexRef = false;

    public bool isPointer = false;
    public bool isDereference = false;

    public VariableExpression(Util.Token token, AST.Node? parent = null, bool parentRequired = true) : base(token)
    {
        this.nodeType = NodeType.VariableExpression;
        this.generator = new Generator.VariableExpression(this);

        this.value = token.value;
        this.parent = parent;

        // if (token.value.Contains("[") && token.value.Contains("]"))
        // {
        //     String[] splitStr = token.value.Split("[");
        //
        //     this.value = splitStr[0];
        //     this.addChild(new NumberExpression(new Util.Token(Util.TokenType.Int, splitStr[1], token.line, token.column + 1), this));
        // }

        if (token.value.StartsWith("&"))
        {
            Console.WriteLine("pointer var ref detected");
            this.isPointer = true;
            this.value = token.value.Remove(this.value.Length - 1, 1);
        }
        else if (token.value.StartsWith("*"))
        {
            Console.WriteLine("dereference detected");
            this.isDereference = true;
            this.value = token.value.Remove(0, 1);
        }

        if (parent != null)
        {
            parent.addChild(this);
        }
        else if (parentRequired)
        {
            throw new ParserException($"Illegal variable expression {this.value}", token);
        }

    }

    public override void addChild(Util.Token child)
    {
        if (this.children.Count() > 0)
        {
            this.children.Last().addChild(child);
            return;
        }
        base.addChild(child);
    }

    public override void addChild(AST.Node child)
    {
        if (child.nodeType != NodeType.VariableExpression && child.nodeType != NodeType.IndexReference)
        {
            throw ParserException.FactoryMethod("An illegal child was added to a variable expression", "remove it", child);
        }

        // if (child.nodeType == NodeType.VariableExpression)
        // {
        //     this.isPointer = true;
        // }

        Console.WriteLine($"adding child to varExpr with type of " + child.nodeType);
        // if (child.nodeType == NodeType.NumberExpression)
        // {
        //     this.numExpr = (NumberExpression)child;
        //     return;
        // }

        if (children.Count() > 0)
        {
            AST.Node lastChild = children.Last();
            child.parent = lastChild;
            lastChild.addChild(child);
            return;
        }

        base.addChild(child);
    }
}
