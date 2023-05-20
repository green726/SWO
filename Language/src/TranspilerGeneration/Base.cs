namespace TranspilerGenerator;

public abstract class Base
{
    public TranspilerGen gen;

    public virtual void generate()
    {
        gen = TranspilerGen.getInstance();
    }
}
