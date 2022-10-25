namespace AST;

public class ExternStatement : Node
{
    // private bool isBody = false;

    public ExternStatement(Util.Token token, AST.Node parent = null) : base(token)
    {
        this.nodeType = NodeType.ExternStatement;
        this.generator = new Generator.ExternStatement(this);
        this.newLineReset = false;

        if (parent != null)
        {
            throw ParserException.FactoryMethod("Illegal non-top level extern statement", "Remove it | Make it top level", this);
        }
    }

    public override void addChild(Node child)
    {
        // if (child.nodeType != AST.Node.NodeType.Prototype && child.nodeType != AST.Node.NodeType.Struct)
        // {
        //     throw ParserException.FactoryMethod("Illegal child added to extern statement", "Remove it", child);
        // }
        //TODO: come up with a way to mark them as implemented/unextendable
        switch (child.nodeType)
        {
            case NodeType.Struct:
                break;
            case NodeType.Prototype:
                Prototype proto = (Prototype)child;
                proto.external = true;
                DebugConsole.WriteAnsi($"[purple]changed proto to {proto.external} extern[/]");
                break;
            default:
                throw ParserException.FactoryMethod("Illegal child added to extern statement", "Remove it", child, this);
        }
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {
        if (child.value != "}")
            throw ParserException.FactoryMethod("Illegal child added to extern statement", "Remove it", child, this);
    }
}
