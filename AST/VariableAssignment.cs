public class VariableAssignment : ASTNode
{
    public string name;
    public TypeAST type;
    public string assignmentOp;
    public string strValue;
    public bool mutable = false;
    private int childLoop = 0;

    public VariableAssignment(Util.Token token, bool mutable) : base(token)
    {
        this.mutable = mutable;
        this.nodeType = NodeType.VariableAssignment;
        Parser.nodes.Add(this);
    }

    public override void addChild(Util.Token child)
    {
        switch (childLoop)
        {
            case 0:
                this.type = new TypeAST(child);
                break;
            case 1:
                this.name = child.value;
                break;
            case 2:
                if (child.type != Util.TokenType.AssignmentOp) throw new ParserException($"expected assignment op but got {child.type}", child);
                this.assignmentOp = child.value;
                break;
            case 3:
                this.strValue = child.value;
                break;
            default:
                throw new ParserException($"Illegal extra items after variable assignment", this);

        }
        childLoop++;

    }

}
