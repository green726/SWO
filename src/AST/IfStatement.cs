

public class IfStatement : ASTNode
{
    public BinaryExpression expression = null;
    // public List<ASTNode> thenBody = new List<ASTNode>();
    public FunctionAST thenFunc;
    public FunctionCall thenCall;
    public ElseStatement elseStat;
    private bool isBody = false;
    // private bool isStat = true;


    public IfStatement(Util.Token token, ASTNode? parent = null) : base(token)
    {
        this.nodeType = NodeType.IfStatement;
        parent?.addChild(this);



        Util.Token thenProtoTok = new Util.Token(Util.TokenType.Keyword, "@then" + Parser.ifFuncNum, token.line, token.column);
        PrototypeAST thenProto = new PrototypeAST(thenProtoTok);
        this.thenFunc = new FunctionAST(thenProto, new List<ASTNode>(), topLevel: false);


        Util.Token thenCallTok = new Util.Token(Util.TokenType.Keyword, "then" + Parser.ifFuncNum, token.line, token.column);
        this.thenCall = new FunctionCall(thenCallTok, null, topLevel: false);


        this.elseStat = new ElseStatement(this, token);

        Parser.ifFuncNum++;

    }

    public override void addChild(ASTNode child)
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
        if (child.type == Util.TokenType.BrackDelimiterOpen)
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
