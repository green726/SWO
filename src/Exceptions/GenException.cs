using System;

public class GenException : Exception
{
    public GenException(string msg, ASTNode node) : base($"{msg} at {node.line}:{node.column}")
    {
    }

    public GenException(string msg, int line, int column) : base($"{msg} at {line}:{column}")
    {
    }

    public GenException(string msg) : base(msg) { }

    public static GenException FactoryMethod(string whatHappened, string recommendedFix, string codeExcerpt, ASTNode node)
    {
        string codeBlock = getCodeBlock(node);
        string input = $"{whatHappened} - ```\n{codeExcerpt}```, \n How You Can Fix This: \n {recommendedFix} \n Error Was Thrown At {node.line}:{node.column}";
        return new GenException(input);
    }

    public static string getCodeBlock(ASTNode node)
    {
        return "";
    }

    // public override string? StackTrace => "";
}
