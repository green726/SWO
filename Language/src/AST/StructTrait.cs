namespace AST;


public class StructTrait : Node
{
    public string traitName { get; set; } = "";

    public StructTrait(Util.Token token) : base(token)
    {

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
