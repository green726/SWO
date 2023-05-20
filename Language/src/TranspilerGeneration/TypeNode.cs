namespace TranspilerGenerator;

public class Type : Base
{
    AST.Type type;

    public Type(AST.Node node)
    {
        this.type = (AST.Type)node;
    }

    public override void generate()
    {
        base.generate();

        gen.write(this.type.value);
    }
}

