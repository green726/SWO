namespace Generator;
using static IRGen;
using LLVMSharp;

public class StructTrait : Base
{
    public AST.StructTrait trait;
    public StructTrait(AST.StructTrait trait)
    {
        this.trait = trait;
    }

    public override void generate()
    {
        base.generate();
    }
}
