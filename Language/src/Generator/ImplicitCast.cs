namespace Generator;

using static IRGen;
using LLVMSharp;

public class ImplicitCast : Expression
{
    private Expression exprGen;
    private TypeInformation desiredType;


    public ImplicitCast(AST.Expression expr, Expression exprGen, TypeInformation desiredType) : base(expr)
    {
        this.expr = expr;
        this.exprGen = exprGen;
        this.desiredType = desiredType;
    }

    public override void generate()
    {
        this.exprGen.generate();
        base.generate();

        if (this.desiredType.isTrait && this.expr.type.isStruct)
        {
            LLVMValueRef traitImpl = gen.getNamedValueInScope(this.expr.value + "_" + this.desiredType.value);
            if (traitImpl.Pointer == IntPtr.Zero)
            {
                throw GenException.FactoryMethod($"Trait implementation for {this.expr.type.value} not found for trait {this.desiredType.value}", $"Add a trait implementation for this struct to be able to pass it as type {this.desiredType.value}", this.expr);
            }

            gen.valueStack.Push(traitImpl);
            return;
        }

        this.expr.generator.generate();
        LLVMValueRef cast = LLVM.BuildCast(gen.builder, LLVMOpcode.LLVMBitCast, gen.valueStack.Pop(), ((GeneratorTypeInformation)this.desiredType).getLLVMType(), "cast");
        gen.valueStack.Push(cast);
    }
}
