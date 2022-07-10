namespace Generator;

public class ElseStatement : Base
{
    global::ElseStatement elseStat;

    public ElseStatement(ASTNode node)
    {
        this.elseStat = (global::ElseStatement)node;
    }

    public override void generate()
    {

    }
}
