public class GenException : Exception
{
    public GenException(string msg, ASTNode node) : base($"{msg} at {node.line}:{node.column}")
    {
    }

    public GenException(string msg, int line, int column) : base($"{msg} at {line}:{column}")
    {
    }

    public override string? StackTrace => "";
}
