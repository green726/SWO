namespace AST;

public class StructImplements : Node
{


    public StructImplements(Util.Token token, AST.Node parent) : base(token)
    {
        this.parent = parent;
        this.parent.addChild(this);
    }
}
