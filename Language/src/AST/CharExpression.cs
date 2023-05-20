namespace AST;

public class CharExpression : Expression
{
    public CharExpression(Util.Token token, AST.Node parent) : base(token, parent)
    {
        this.nodeType = NodeType.CharExpression;
        this.generator = new Generator.CharExpression(this);
        this.transpilerGenerator = new TranspilerGenerator.CharExpression(this);
        this.parent = parent;
        this.parent.addChild(this);


        if (token.value.Length != 1)
        {
            throw ParserException.FactoryMethod("Character literal with more than one character detected", "Remove the extra characters", token, this.parent);
        }
        this.value = token.value[0];
    }

}
