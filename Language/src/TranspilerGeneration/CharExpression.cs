namespace TranspilerGenerator;

public class CharExpression : Expression
{
    public AST.CharExpression charExpression;

    public CharExpression(AST.CharExpression charExpression) : base(charExpression)
    {
        this.charExpression = charExpression;
    }

    public override void generate()
    {
        base.generate();

        gen.write("'" + charExpression.value.ToString() + "'");
    }

}
