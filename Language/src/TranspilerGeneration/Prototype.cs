namespace TranspilerGenerator;

public class Prototype : Base
{
    AST.Prototype proto;

    public Prototype(AST.Node node)
    {
        this.proto = (AST.Prototype)node;
    }

    public override void generate()
    {
        base.generate();

        if (Config.target.function.declaration.returnTypeLocation == ConfigModel.ReturnTypeLocation.Start)
        {

            if (proto.returnType.value == "null")
            {
                gen.write($"{Config.target.function.declaration.marker.value}{proto.name}{Config.target.function.declaration.args.delimiters[0]}");
            }
            else
            {
                gen.write($"{proto.returnType.value} {Config.target.function.declaration.marker.value}{proto.name}{Config.target.function.declaration.args.delimiters[0]}");
            }

        }

        int idx = 0;
        foreach (KeyValuePair<string, AST.Type> arg in proto.arguments)
        {
            gen.write($"{arg.Key} ");
            arg.Value.transpilerGenerator.generate();
            if (idx != proto.arguments.Count - 1)
            {
                gen.write($"{Config.target.function.declaration.args.seperator} ");
            }
            idx++;
        }
        gen.write(Config.target.function.declaration.args.delimiters[1] + " ");
    }
}
