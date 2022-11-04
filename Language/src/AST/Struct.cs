namespace AST;


public class Struct : Node
{
    public List<AST.Node> properties = new List<AST.Node>();

    public string name = "";

    public Struct(Util.Token token, AST.Node parent) : base(token)
    {
        this.nodeType = NodeType.Struct;
        this.generator = new Generator.Struct(this);
        this.newLineReset = false;

        this.parent = parent;

        parser.nodes.Add(this);
    }

    public Struct(Util.Token token) : base(token)
    {
        this.nodeType = NodeType.Struct;
        this.generator = new Generator.Struct(this);
        this.newLineReset = false;

        parser.nodes.Add(this);
    }
    public VariableDeclaration getProperty(string propName, AST.Node caller)
    {
        foreach (AST.Node node in properties)
        {
            AST.VariableDeclaration varDec = (AST.VariableDeclaration)node;
            if (varDec.name == propName)
            {
                return varDec;
            }
        }

        throw ParserException.FactoryMethod("An unknown property of a struct was referenced", "Remove the property reference, or change it to one that is a part of the struct", caller, this);
    }

    public VariableDeclaration getProperty(string propName)
    {
        List<VariableDeclaration> decList = (List<VariableDeclaration>)properties.Where((prop) =>
        {
            VariableDeclaration varDec = (VariableDeclaration)prop;
            return varDec.name == propName;
        });

        if (decList.Count == 0)
        {
            throw ParserException.FactoryMethod("An unknown property of a struct was referenced", "Remove the property reference, or change it to one that is a part of the struct", this);
        }

        return decList[0];
    }

    public int getPropertyIndex(string propName)
    {
        int idx = 0;
        foreach (AST.Node node in properties)
        {
            AST.VariableDeclaration varDec = (AST.VariableDeclaration)node;
            if (varDec.name == propName)
            {
                return idx;
            }
            idx++;
        }
        throw ParserException.FactoryMethod("An unknown property of a struct was referenced", "Remove the property reference, or change it to one that is a part of the struct", this);
    }

    public override void addChild(Node child)
    {
        if (this.name == null && child.nodeType == AST.Node.NodeType.VariableExpression)
        {
            VariableExpression varExpr = (VariableExpression)child;
            this.name = varExpr.value;
            parser.typeList.Add(this.name);
            parser.declaredStructs.Add(this.name, this);
            base.addChild(child);
            return;
        }
        if (child.nodeType != NodeType.VariableDeclaration)
        {
            throw new ArgumentException("non var dec added to str");
        }
        DebugConsole.WriteAnsi("[green]adding var dec to struct[/]");
        AST.VariableDeclaration varDec = (AST.VariableDeclaration)child;
        properties.Add(child);
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {
        if (this.name == "")
        {
            this.name = child.value;
            parser.typeList.Add(this.name);
            parser.declaredStructs.Add(this.name, this);
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
