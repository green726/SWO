namespace TranspilerGenerator;

public class VariableDeclaration : Base
{
    public AST.VariableDeclaration varDec;

    public VariableDeclaration(AST.Node node)
    {
        this.varDec = (AST.VariableDeclaration)node;
    }

    public override void generate()
    {
        base.generate();

        if (Config.settings.variable.declaration.keyword.forced)
        {
            if (varDec.mutable)
            {
                gen.write(Config.settings.variable.declaration.keyword.mutable);
            }
            else
            {
                gen.write(Config.settings.variable.declaration.keyword.constant);
            }
        }

        gen.write(" ");
        varDec.type.transpilerGenerator.generate();
        gen.write($" {varDec.name}");
        if (varDec.defaultValue != null && varDec.defaultValue.nodeType != AST.Node.NodeType.Empty && varDec.defaultValue.nodeType != AST.Node.NodeType.NullExpression) {
            gen.write(" = ");
            varDec.defaultValue.transpilerGenerator.generate();
        }
        // gen.write(";");
    }
}
