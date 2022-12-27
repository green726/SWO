namespace AST;

using System.Collections.Generic;

public class Function : AST.Node
{
    public Prototype prototype;
    public List<AST.Node> body;

    public bool generated = false;

    public bool multiLine = false;

    //NOTE: Constructor 1:
    public Function(Prototype prototype) : base(prototype)
    {
        this.nodeType = NodeType.Function;
        this.generator = new Generator.Function(this);

        if (body == null) body = new List<AST.Node>();

        if (prototype.external)
        {
            throw ParserException.FactoryMethod("Prototype marked external implemented with body", "Unmark it as external, or remove the body/implementation of the prototype", this, prototype);
        }

        if (prototype.parent == null || prototype.parent.nodeType == NodeType.Empty)
        {
            parser.nodes.Add(this);
        }
        else
        {
            DebugConsole.WriteAnsi("[blue]func named " + prototype.name + " with parent: " + prototype.parent.nodeType + "[/]");
            this.parent = prototype.parent;
        }

        this.prototype = prototype;
        //NOTE: prototype check export must be above setting parent
        this.prototype.checkExport();
        this.prototype.parent = this;
        this.body = new List<Node>();

        if (this.parent.nodeType != NodeType.Empty)
        {
            this.parent.addChild(this);
        }

        // if (Config.settings.function.declaration.reorder && !Parser.declaredFunctionDict.ContainsKey(prototype.name))
        // {
        //     Parser.declaredFunctionDict.Add(prototype.name, this);
        // }
    }

    //NOTE: Constructor 2:
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
        base.addChild(child);
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
    }


}
