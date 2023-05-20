namespace TranspilerGenerator;

public class IfStatement : Base
{
    AST.IfStatement ifStat;

    public IfStatement(AST.Node node)
    {
        this.ifStat = (AST.IfStatement)node;
    }

    public override void generate()
    {
        base.generate();
    }

}
