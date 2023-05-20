namespace TranspilerGenerator;

public class Function : Base
{
    AST.Function func;

    private bool topLevelRet = false; //has a return been created as a direct child of the function?


    public Function(AST.Node node)
    {
        this.func = (AST.Function)node;
    }

    public override void generate()
    {
        base.generate();

        func.prototype.transpilerGenerator.generate();

        gen.writeLine($"{Config.target.function.declaration.body.delimiters[0]}");

        int idx = 0;
        foreach (AST.Node bodyNode in func.body)
        {
            gen.write("\t");
            bodyNode.transpilerGenerator.generate();

            if (idx != func.body.Count - 1)
            {
                gen.checkSemiColonAndNL(bodyNode);
            }
            else
            {
                gen.checkSemiColon(bodyNode);
            }
            idx++;
        }

        gen.writeLine();
        gen.writeLine($"{Config.target.function.declaration.body.delimiters[1]}");
    }

}
