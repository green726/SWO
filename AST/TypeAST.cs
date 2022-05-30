public class TypeAST : ASTNode
{
    public string value;
    public Util.ClassType defaultType;

    public TypeAST(Util.Token token)
    {
        this.value = token.value;


    }

    public TypeAST(string val)
    {
        this.value = val;

    }
}
