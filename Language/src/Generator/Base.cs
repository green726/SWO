namespace Generator;

public abstract class Base
{
    public IRGen gen;

    public virtual void generate()
    {
        gen = IRGen.getInstance();
    }
}
