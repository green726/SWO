
public class ElseStatement : ASTNode
{
    public FunctionAST elseFunc;
    public IfStatement ifParent;

    public ElseStatement(ASTNode parent, Util.Token token) : base(token)
    {

        this.parent = parent;
        PrototypeAST elseProto = new PrototypeAST(token, false);
        this.elseFunc = new FunctionAST(elseProto, new List<ASTNode>());
        this.ifParent = ifParent;


    }
}
