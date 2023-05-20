namespace TranspilerGenerator;

public class ElseIfStatement : Base
{

    public AST.ElseIfStatement elseIf { get; set; }
    public ElseIfStatement(AST.ElseIfStatement stat)
    {
        elseIf = stat;
    }

    public override void generate()
    {
        base.generate();
    }

}
