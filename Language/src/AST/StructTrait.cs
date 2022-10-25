namespace AST;


public class StructTrait : Node
{
    public StructTrait(Util.Token token, AST.Node parent) : base(token)
    {
        this.parent = parent;
        this.parent.addChild(this);
    }
}
