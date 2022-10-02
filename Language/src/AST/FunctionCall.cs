namespace AST;

using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FunctionCall : Expression
{
    public string? functionName;
    public List<AST.Expression> args;

    public FunctionCall(Util.Token token, List<AST.Expression>? args, AST.Node? parent = null, bool topLevel = false) : base(token)
    {
        this.newLineReset = true;
        this.nodeType = NodeType.FunctionCall;
        this.generator = new Generator.FunctionCall(this);

        string builtinName = token.value.Substring(0, (token.value.Length - 1));

        this.functionName = token.value;

        this.args = args ??= new List<AST.Expression>();

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

    public string generateAltName()
    {
        StringBuilder altNameSb = new StringBuilder();

        foreach (AST.Expression argExpr in args)
        {
            DebugConsole.Write(argExpr.nodeType);
            DebugConsole.Write(argExpr.type.value);
            altNameSb.Append("_" + argExpr.type.value);
        }
        return altNameSb.ToString();
    }

    public override void addChild(AST.Node child)
    {
        base.addChild(child);
        args.Add((AST.Expression)child);
    }

    public override void removeChild(AST.Node child)
    {
        args.Remove((AST.Expression)child);
        base.removeChild(child);
    }

    public void addChildAtStart(AST.Node child)
    {
        args.Insert(0, (AST.Expression)child);
        base.addChild(child);
    }
}
