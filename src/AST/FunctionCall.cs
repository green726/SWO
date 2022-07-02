using System.Collections.Generic;
using System.Linq;

public class FunctionCall : ASTNode
{
    public string? functionName;
    public bool builtIn = false;
    public List<ASTNode> args;

    public FunctionCall(Util.Token token, List<ASTNode>? args, bool? builtInExpected = false, ASTNode? parent = null, bool topLevel = false) : base(token)

    {
        this.nodeType = NodeType.FunctionCall;
        string builtinName = token.value.Substring(0, (token.value.Length - 1));

        if (Util.builtinFuncs.Contains(builtinName))
        {
            this.builtIn = true;
        }
        if (builtInExpected == true && this.builtIn == false)
        {
            throw new ParserException($"builtin function expected but {builtinName} is not one", token);
        }

        this.functionName = this.builtIn ? builtinName : token.value;
        this.args = args ??= new List<ASTNode>();

        //NOTE: commented out below is to throw in an anonymous function
        // PrototypeAST proto = new PrototypeAST();
        // FunctionAST func = new FunctionAST(proto, this);

        if (parent != null)
        {
            this.parent = parent;
            this.parent.addChild(this);
            return;
        }

        else if (topLevel)
        {
            // Parser.nodes.Add(this);

        }
    }

    public override void addChild(ASTNode child)
    {
        args.Add(child);
    }

    public override void removeChild(ASTNode child)
    {
        args.Remove(child);
        base.removeChild(child);
    }

    public void addChildAtStart(ASTNode child)
    {
        args.Insert(0, child);
    }
}
