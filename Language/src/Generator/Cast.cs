namespace Generator;

using LLVMSharp;
using static IRGen;

public class Cast : Expression
{
    public AST.Cast cast;
    public Cast(AST.Expression expr) : base(expr)
    {
        this.cast = (AST.Cast)expr;
    }

    public override void generate()
    {
        base.generate();

        if (cast.desiredType.isTrait && cast.target.type.isStruct)
        {
            //TODO: get the struct's trait implementation

            LLVMValueRef traitImpl = gen.getNamedValueInScope(cast.target.type.value + "_" + cast.desiredType.value);
            if (traitImpl.Pointer == IntPtr.Zero)
            {
                throw GenException.FactoryMethod($"Trait implementation for {cast.target.type.value} not found for trait {cast.desiredType.value}", $"Add a trait implementation for this struct to be able to pass it as type {cast.desiredType.value}", cast);
            }

            gen.valueStack.Push(traitImpl);
            return;
        }

        LLVMValueRef casted = LLVM.BuildCast(gen.builder, LLVMOpcode.LLVMBitCast, cast.target.value, ((GeneratorTypeInformation)cast.desiredType).getLLVMType(), "cast");
        gen.valueStack.Push(casted);

    }
}
