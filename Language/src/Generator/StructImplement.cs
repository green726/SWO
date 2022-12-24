namespace Generator;

using static IRGen;
using LLVMSharp;

public class StructImplement : Base
{
    public AST.StructImplement implement;

    public StructImplement(AST.StructImplement implement)
    {
        this.implement = implement;
    }

    public override void generate()
    {
        base.generate();
    }
}
