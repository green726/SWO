namespace Generator;

using LLVMSharp;
using static IRGen;

public abstract class Expression : Base
{
    public AST.Expression expr;
    public GeneratorTypeInformation typeInfo;

    protected Expression(AST.Expression expr)
    {
        this.expr = expr;
        // this.typeInfo = (GeneratorTypeInformation)expr.type;
    }

    public override void generate()
    {
        DebugConsole.Write(expr.type);
        this.typeInfo = (GeneratorTypeInformation)expr.type;

        if (expr.implicitCast != null)
        {
            
        }

        base.generate();
    }

}

