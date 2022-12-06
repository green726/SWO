namespace Generator;

public class ParenEncapsulation : Expression
{
    public AST.ParenEncapsulation encap { get; set; }

    public ParenEncapsulation(AST.ParenEncapsulation expr) : base(expr)
    {
        this.encap = expr;
    }

    public override void generate()
    {
        base.generate();

        encap.containedExpression.generator.generate();
    }
}
