namespace AST;

using System.Collections.Generic;

public class Function : AST.Node
{
    public Prototype prototype;
    public List<AST.Node> body;

    public bool generated = false;


    public Function(Prototype prototype, List<AST.Node>? body = null, bool topLevel = true) : base(prototype)
    {
        this.nodeType = NodeType.Function;
        this.generator = new Generator.Function(this);


        if (body == null) body = new List<AST.Node>();
        this.prototype = prototype;
        this.body = body;

        if (topLevel)
        {
            Parser.nodes.Add(this);
        }

        if (Config.options.function.declaration.reorder)
        {
            Parser.declaredFunctionDict.Add(prototype.name, this);
        }
    }

    public Function(Prototype prototype, AST.Node body, bool topLevel = true) : base(prototype)

    {
        this.nodeType = NodeType.Function;
        this.generator = new Generator.Function(this);

        this.prototype = prototype;
        this.body = new List<AST.Node>();
        this.body.Add(body);

        if (topLevel)
        {
            Parser.nodes.Add(this);
        }

        if (Config.options.function.declaration.reorder)
        {
            Parser.declaredFunctionDict.Add(prototype.name, this);
        }
    }

    public override void removeChild(AST.Node child)
    {
        base.removeChild(child);
        this.body.Remove(child);
    }

    public override void addChild(AST.Node child)
    {
        base.addChild(child);
        this.body.Add(child);
    }

}
