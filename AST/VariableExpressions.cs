public class VariableExpression : ASTNode
{
    public string varName = "";

    public VariableExpression(Util.Token token, ASTNode? parent = null) : base(token)
    {
        varName = token.value;
        bool exists = false;

        foreach (VariableAssignment varAss in Parser.globalVarAss)
        {
            if (this.varName == varAss.name)
            {
                exists = true;
                break;
            }
        }

        if (exists == false)
        {
            throw new ParserException($"Unknown variable referenced {this.varName}", token);
        }

        if (parent != null)
        {
            parent.addChild(this);
        }
        else
        {
            throw new ParserException($"Illegal variable expression {this.varName}", token);
        }
    }
}
