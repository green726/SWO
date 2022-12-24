namespace AST;

public class StructTrait : Node
{
    public string traitName { get; set; } = "";
    public Prototype prototype { get; set; }
    public Function function { get; set; }

    public StructTrait(Util.Token token) : base(token)
    {
        this.nodeType = NodeType.Trait;
        this.generator = new Generator.StructTrait(this);
        this.newLineReset = false;

        parser.nodes.Add(this);
    }

    public override void addChild(AST.Node child)
    {
        if (this.prototype == null)
        {
            this.prototype = (Prototype)child;
        }
        else
        {
            throw ParserException.FactoryMethod($"Illegal child of type ({child.nodeType}) added to trait", "Remove it", child, this);
        }
    }

    public override void addChild(Util.Token child)
    {
        if (this.traitName != "")
        {
            throw ParserException.FactoryMethod("Illegal token added to struct trait definition", "Remove it", child, this);
        }
        this.traitName = child.value;
        base.addChild(child);
    }
}
