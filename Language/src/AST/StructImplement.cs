namespace AST;

public class StructImplement : Node
{
    public StructTrait trait;
    public Struct str;

    public StructImplement(Util.Token token, AST.Node parent) : base(token)
    {
        this.parent = parent;
        this.parent.addChild(this);
    }
}
