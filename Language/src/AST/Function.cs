namespace AST;

using System.Collections.Generic;

public class Function : AST.Node
{
    public Prototype prototype;
    public List<AST.Node> body;

    public bool generated = false;

    public bool multiLine = false;

    //NOTE: Constructor 1:
    public Function(Prototype prototype, List<AST.Node>? body = null, bool topLevel = true) : base(prototype)
    {
        this.nodeType = NodeType.Function;
        this.generator = new Generator.Function(this);


        if (body == null) body = new List<AST.Node>();

        DebugConsole.WriteAnsi("[purple]proto extern: " + prototype.external + "[/]");
        if (prototype.external)
        {
            throw ParserException.FactoryMethod("Prototype marked external implemented with body", "Unmark it as external, or remove the body/implementation of the prototype", this, prototype);
        }
        this.prototype = prototype;
        this.prototype.parent = this;
        this.body = body;

        if (topLevel)
        {
            Parser.getInstance().nodes.Add(this);
        }

        // if (Config.settings.function.declaration.reorder && !Parser.declaredFunctionDict.ContainsKey(prototype.name))
        // {
        //     Parser.declaredFunctionDict.Add(prototype.name, this);
        // }
    }

    //NOTE: Constructor 2:
    public Function(Prototype prototype, AST.Node body, bool topLevel = true) : base(prototype)
    {

        if (prototype.external)
        {
            throw ParserException.FactoryMethod("Prototype marked external implemented with body", "Unmark it as external, or remove the body/implementation of the prototype", prototype);
        }


        this.newLineReset = true;
        this.multiLine = false;
        this.nodeType = NodeType.Function;
        this.generator = new Generator.Function(this);

        this.prototype = prototype;
        this.body = new List<AST.Node>();
        this.body.Add(body);

        if (topLevel)
        {
            Parser.getInstance().nodes.Add(this);
        }

        // if (Config.settings.function.declaration.reorder)
        // {
        //     Parser.declaredFunctionDict.Add(prototype.name, this);
        // }
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

    public override void addChild(Util.Token child)
    {
        if (children.Count() == 0)
        {
            if (child.value == "{")
            {
                this.multiLine = true;
                this.newLineReset = false;
            }
        }
        else
        {
            throw ParserException.FactoryMethod("Illegal token added to function", "Delete the token", child, this, true);
        }
        base.addChild(child);
    }


}
