public class FunctionCall : ASTNode
{
    public string? functionName;
    public bool builtIn = false;
    public List<ASTNode> args;

    public FunctionCall(Util.Token token, List<ASTNode>? args, bool? builtInExpected = false, ASTNode? parent = null)
    {
        this.nodeType = NodeType.FunctionCall;

        if (Util.builtinFuncs.Contains(token.value))
        {
            this.builtIn = true;
        }
        if (builtInExpected == true && this.builtIn == false)
        {
            throw new ArgumentException("builtin function expected but name does not exist");
        }

        this.functionName = token.value;
        this.args = args ??= new List<ASTNode>();

        PrototypeAST proto = new PrototypeAST();
        FunctionAST func = new FunctionAST(proto, this);

        if (parent != null)
        {
            parent.addChild(this);
            return;
        }

        Parser.nodes.Add(func);
    }

    public override void addChild(ASTNode child)
    {
        args.Add(child);
    }

    public override void removeChild(ASTNode child)
    {
        args.Remove(child);
    }

    public void addChildAtStart(ASTNode child)
    {
        args.Insert(0, child);
    }
}
