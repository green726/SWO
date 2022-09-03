namespace AST;


public class Struct : Node
{
    public List<AST.Node> properties = new List<AST.Node>();
    public List<string> propertiesNames = new List<string>();

    public string name = null;

    public Struct(Util.Token token, AST.Node? parent = null) : base(token)
    {
        this.nodeType = NodeType.Struct;
        this.generator = new Generator.Struct(this);
        this.newLineReset = false;

        this.parent = parent;

        Parser.nodes.Add(this);
    }

    public int getPropIndex(string propName)
    {
        return propertiesNames.IndexOf(propName);
    }

    public override void addChild(Node child)
    {
        if (this.name == null && child.nodeType == AST.Node.NodeType.VariableExpression)
        {
            VariableExpression varExpr = (VariableExpression)child;
            this.name = varExpr.value;
            Parser.typeList.Add(this.name);
            base.addChild(child);
            return;
        }
        if (child.nodeType != NodeType.VariableDeclaration)
        {
            throw new ArgumentException("non var dec added to str");
        }
        AST.VariableDeclaration varDec = (AST.VariableDeclaration)child;
        properties.Add(child);
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {
        if (this.name == null)
        {
            this.name = child.value;
            Parser.typeList.Add(this.name);
        }
        else if (child.value == "{" || child.value == "}")
        {
        }
        else
        {
            throw ParserException.FactoryMethod("Illegal token added to struct", "Remove the token", child, this);
        }
        base.addChild(child);
    }
}
