namespace AST;


public class ElseStatement : AST.Node
{
    // public Function elseFunc;
    // public FunctionCall elseCall;

    public List<AST.Node> elseBody;


    public ElseStatement(AST.Node parent, Util.Token token) : base(token)
    {
        this.nodeType = NodeType.ElseStatement;
        this.generator = new Generator.ElseStatement(this);

        this.parent = parent.parent;

        this.elseBody = new List<AST.Node>();

        // Util.Token elseProtoTok = new Util.Token(Util.TokenType.Keyword, "@else" + Parser.ifFuncNum, token.line, token.column);
        // Prototype elseProto = new Prototype(elseProtoTok);
        // this.elseFunc = new Function(elseProto, new List<AST.Node>(), topLevel: false);
        // this.elseFunc.utilFunc = true;


        // Util.Token thenCallTok = new Util.Token(Util.TokenType.Keyword, "else" + Parser.ifFuncNum, token.line, token.column);
        // this.elseCall = new FunctionCall(thenCallTok, null, topLevel: false);

    }

    public override void addChild(AST.Node child)
    {
        elseBody.Add(child);
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {
        if (child.value != "{" && child.value != "}")
        {

            throw ParserException.FactoryMethod($"Illegal child was added to an else statement", "No recommended fix", child);
        }
        base.addChild(child);
    }
}
