namespace AST;


public class ElseStatement : AST.Node
{
    public Function elseFunc;
    public FunctionCall elseCall;

    public ElseStatement(AST.Node parent, Util.Token token) : base(token)
    {
        this.nodeType = NodeType.ElseStatement;

        this.parent = parent;
        Util.Token elseProtoTok = new Util.Token(Util.TokenType.Keyword, "@else" + Parser.ifFuncNum, token.line, token.column);
        Prototype elseProto = new Prototype(elseProtoTok);
        this.elseFunc = new Function(elseProto, new List<AST.Node>(), topLevel: false);


        Util.Token thenCallTok = new Util.Token(Util.TokenType.Keyword, "else" + Parser.ifFuncNum, token.line, token.column);
        this.elseCall = new FunctionCall(thenCallTok, null, topLevel: false);

    }

    public override void addChild(AST.Node child)
    {
        elseFunc.addChild(child);
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {
        throw ParserException.FactoryMethod($"Illegal child was added to an if statement", "No recommended fix", child);
    }
}
