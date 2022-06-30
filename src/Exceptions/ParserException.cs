using System;

public class ParserException : Exception
{
    public ParserException(string msg, ASTNode node) : base($"{msg} at {node?.line}:{node?.column}")
    {
    }

    public ParserException(string msg, Util.Token token) : base($"{msg} at {token?.line}:{token?.column}")
    {
    }

    public ParserException(string msg, int line, int column) : base($"{msg} at {line}:{column}")
    {
    }

    // public override string? StackTrace => "";

}
