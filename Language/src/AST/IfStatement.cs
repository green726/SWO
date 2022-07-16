namespace AST;

public class IfStatement : AST.Node
{
    public BinaryExpression expression = null;
    // public List<AST.Node> thenBody = new List<AST.Node>();
    public Function thenFunc;
    public FunctionCall thenCall;
    public ElseStatement elseStat;
    private bool isBody = false;
    // private bool isStat = true;


    public IfStatement(Util.Token token, AST.Node? parent = null) : base(token)
    {
        this.nodeType = NodeType.IfStatement;
        this.generator = new Generator.IfStatement(this);

        parent?.addChild(this);


        Util.Token thenProtoTok = new Util.Token(Util.TokenType.Keyword, "@then" + Parser.ifFuncNum, token.line, token.column);
        Prototype thenProto = new Prototype(thenProtoTok);
        this.thenFunc = new Function(thenProto, new List<AST.Node>(), topLevel: false);


        Util.Token thenCallTok = new Util.Token(Util.TokenType.Keyword, "then" + Parser.ifFuncNum, token.line, token.column);
        this.thenCall = new FunctionCall(thenCallTok, null, topLevel: false);


        this.elseStat = new ElseStatement(this, token);

        Parser.ifFuncNum++;

    }

    public override void addChild(AST.Node child)
    {
        if (expression == null)
        {
            Parser.checkNode(child, new NodeType[] { NodeType.BinaryExpression, NodeType.NumberExpression, NodeType.VariableExpression });
            if (child.nodeType == NodeType.BinaryExpression)
            {
                expression = (BinaryExpression)child;
            }
            else
            {
                base.addChild(child);
                return;
            }
        }
        else if (isBody == true)
        {
            thenFunc.addChild(child);
        }
        else
        {
            throw new ParserException($"Attempted to add multiple conditions to if statement (currently unsupported)", child);
        }
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {
        // if (child.type == Util.TokenType.ParenDelimiterClose)
        // {
        //     isStat = false;
        // }
        if (child.value == Config.settings.ifModel.body.delimeters[0]/* Util.TokenType.BrackDelimiterOpen */)
        {
            isBody = true;
        }
        else
        {
            throw new ParserException($"illegal child ({child.value}) added to if statement", child);
        }
        base.addChild(child);
    }
}
