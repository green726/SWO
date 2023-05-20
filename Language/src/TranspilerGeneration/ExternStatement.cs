namespace TranspilerGenerator;


public class ExternStatement : Base
{
    public AST.ExternStatement externStat;
    public ExternStatement(AST.Node node)
    {
        this.externStat = (AST.ExternStatement)node;
    }

    public override void generate()
    {

    }
}
