namespace TranspilerGenerator;

public class FunctionCall : Base
{
    AST.FunctionCall funcCall;

    static string[] builtinNames = { "sizeof" };

    public FunctionCall(AST.Node node)
    {
        this.funcCall = (AST.FunctionCall)node;
    }


    public override void generate()
    {
        base.generate();

        gen.write($"{funcCall.functionName}{Config.settings.function.calling.args.delimiters[0]}");

        foreach (AST.Expression arg in funcCall.args) {
            arg.transpilerGenerator.generate();
            gen.write(Config.settings.function.calling.args.seperator);
        }
        
        gen.write(Config.settings.function.calling.args.delimiters[1]);
    }

}
