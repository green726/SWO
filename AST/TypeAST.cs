public class TypeAST : ASTNode
{
    public string value;
    public Util.ClassType defaultType;

    public TypeAST(Util.Token token) : base(token)
    {
        this.nodeType = NodeType.TypeAST;
        this.value = token.value;


    }

}
