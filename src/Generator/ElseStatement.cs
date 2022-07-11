namespace Generator;

public class ElseStatement : Base
{
    AST.ElseStatement elseStat;

    public ElseStatement(AST.Node node)
    {
        this.elseStat = (AST.ElseStatement)node;
    }

    public override void generate()
    {

    }
}
