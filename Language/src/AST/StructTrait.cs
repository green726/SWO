namespace AST;

public class StructTrait : Node
{
    public string name { get; set; } = "";
    public List<Prototype> protos { get; set; } = new List<Prototype>();

    public StructTrait(Util.Token token, AST.Node parent) : base(token)
    {
        this.nodeType = NodeType.Trait;
        this.generator = new Generator.StructTrait(this);
        this.newLineReset = false;

        if (parent.nodeType != AST.Node.NodeType.Empty)
        {
            this.parent = parent;
            this.parent.addChild(this);
        }

    }

    public override void addChild(AST.Node child)
    {
        if (child.nodeType == NodeType.Prototype)
        {
            this.protos.Add((Prototype)child);
        }
        else
        {
            throw ParserException.FactoryMethod($"Illegal child of type ({child.nodeType}) added to trait | Only prototypes may be added to traits", "Remove it", child, this);
        }
    }

    public override void addChild(Util.Token child)
    {
        if (this.name != "" && child.type == Util.TokenType.Keyword)
        {
            throw ParserException.FactoryMethod("Illegal token added to struct trait definition", "Remove it", child, this);
        }
        parser.declaredStructTraits.Add(this.name, this);
        this.name = child.value;
        base.addChild(child);
    }
}
