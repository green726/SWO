namespace AST;

using System.Collections.Generic;
using System.Linq;

public class FunctionCall : Expression
{
    public string? functionName;
    public List<AST.Node> args;

    public FunctionCall(Util.Token token, List<AST.Node>? args, AST.Node? parent = null, bool topLevel = false) : base(token)

    {
        this.newLineReset = true;
        this.nodeType = NodeType.FunctionCall;
        this.generator = new Generator.FunctionCall(this);

        string builtinName = token.value.Substring(0, (token.value.Length - 1));

        this.functionName = token.value;
        this.args = args ??= new List<AST.Node>();

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

    public override void addChild(AST.Node child)
    {
        base.addChild(child);
        args.Add(child);
    }

    public override void removeChild(AST.Node child)
    {
        args.Remove(child);
        base.removeChild(child);
    }

    public void addChildAtStart(AST.Node child)
    {
        args.Insert(0, child);
        base.addChild(child);
    }
}
