
public class ElseStatement : ASTNode
{
    public FunctionAST elseFunc;
    public FunctionCall elseCall;

    public ElseStatement(ASTNode parent, Util.Token token) : base(token)
    {
        this.nodeType = NodeType.ElseStatement;

        this.parent = parent;
        Util.Token elseProtoTok = new Util.Token(Util.TokenType.Keyword, "@else" + Parser.ifFuncNum, token.line, token.column);
        PrototypeAST elseProto = new PrototypeAST(elseProtoTok);
        this.elseFunc = new FunctionAST(elseProto, new List<ASTNode>(), topLevel: false);


        Util.Token thenCallTok = new Util.Token(Util.TokenType.Keyword, "else" + Parser.ifFuncNum, token.line, token.column);
        this.elseCall = new FunctionCall(thenCallTok, null, topLevel: false);

    }

    public override void addChild(ASTNode child)
    {
        elseFunc.addChild(child);
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {

        throw new ParserException($"illegal child ({child.value}) added to if statement", child);
    }
}
