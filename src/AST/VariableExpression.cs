public class VariableExpression : ASTNode
{
    public string varName = "";

    public VariableExpression(Util.Token token, ASTNode? parent = null, bool parentRequired = true) : base(token)
    {
        varName = token.value;
        bool exists = false;

        //NOTE: below is commented out b/c i think that LLVm IR will do it for me
        // foreach (VariableAssignment varAss in Parser.globalVarAss)
        // {
        //     if (this.varName == varAss.name)
        //     {
        //         exists = true;
        //         break;
        //     }
        // }
        //
        // if (exists == false)
        // {
        //     throw new ParserException($"Unknown variable referenced {this.varName}", token);
        // }
        if (parent != null)
        {
            parent.addChild(this);
        }
        else if (parentRequired)
        {
            throw new ParserException($"Illegal variable expression {this.varName}", token);
        }
    }
}
