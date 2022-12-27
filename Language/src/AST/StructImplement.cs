namespace AST;

public class StructImplement : Node
{
    public StructTrait trait;
    public Struct str;

    public List<Function> functions = new List<Function>();

    public StructImplement(Util.Token token, AST.Node parent) : base(token)
    {
        this.nodeType = NodeType.Implement;
        this.parent = parent;
        this.str = (AST.Struct)parent;
        this.str.implements.Add(this);
        this.parent.addChild(this);
        this.generator = new Generator.StructImplement(this);
    }

    public void modifyProtoName(Prototype proto)
    {
        DebugConsole.WriteAnsi("[green]modifying proto named " + proto.name + "[/]");
        proto.name = str.name + "_" + proto.name;
    }

    public void addThisKeyword(Prototype proto)
    {
        AST.Type strType = new Type(new Util.Token(Util.TokenType.Keyword, str.name, proto.line, proto.column));
        proto.arguments.Add("this", strType);
        // parser.addNamedValueInScope("this", (ParserTypeInformation)strType, this);
        DebugConsole.WriteAnsi($"[red]added \"this\" to proto named {proto.name}[/]");
    }

    public override void addChild(Util.Token child)
    {
        base.addChild(child);

        if (this.trait == null && child.type == Util.TokenType.Keyword)
        {
            this.trait = parser.declaredStructTraits[child.value];
        }
        else if (child.type == Util.TokenType.Keyword)
        {
            throw ParserException.FactoryMethod("Illegal token added to struct implement definition", "Remove it", child, this);
        }
    }

    public override void addChild(Node child)
    {
        base.addChild(child);

        if (child.nodeType == NodeType.Prototype)
        {
            addThisKeyword((Prototype)child);
        }
        else if (child.nodeType == NodeType.Function)
        {
            Function childFunc = (Function)child;
            modifyProtoName(childFunc.prototype);
            this.functions.Add(childFunc);
        }
        else
        {
            throw ParserException.FactoryMethod("Illegal child added to struct implement", "Remove it", child, this);
        }
    }
}
