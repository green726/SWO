namespace AST;

using System.Collections.Generic;
using System.Linq;

public class FunctionCall : AST.Node
{
    public string? functionName;
    public bool builtIn = false;
    public List<AST.Node> args;

    public FunctionCall(Util.Token token, List<AST.Node>? args, bool? builtInExpected = false, AST.Node? parent = null, bool topLevel = false) : base(token)

    {
        this.nodeType = NodeType.FunctionCall;
        this.generator = new Generator.FunctionCall(this);

        string builtinName = token.value.Substring(0, (token.value.Length - 1));

        if (Util.builtinFuncs.Contains(builtinName))
        {
            this.builtIn = true;
        }
        if (builtInExpected == true && this.builtIn == false)
        {
            throw ParserException.FactoryMethod($"Builtin function call expected but no builtin function with a matching name was found", $"Fix a possible typo? \n Remove the \"{Config.settings.function.calling.builtin.marker.value}\" at the end of the function call to unmark it as builting", token);
        }

        this.functionName = this.builtIn ? builtinName : token.value;
        this.args = args ??= new List<AST.Node>();

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
