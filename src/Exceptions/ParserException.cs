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

    public ParserException(string msg) : base(msg)
    {

    }

    public static ParserException FactoryMethod(string whatHappened, string recommendedFix, Util.Token token)
    {
        string codeBlock = getCodeBlock(token);
        string input = $"{whatHappened} - ```\n{codeBlock}```, \n How You Can Fix This: \n {recommendedFix} \n Error Was Thrown At {token.line}:{token.column}";
        return new ParserException(input);
    }

    public static ParserException FactoryMethod(string whatHappened, string recommendedFix, ASTNode node)
    {
        string codeBlock = getCodeBlock(node);
        string input = $"{whatHappened} - ```\n{codeBlock}```, \n How You Can Fix This: \n {recommendedFix} \n Error Was Thrown At {node.line}:{node.column}";
        return new ParserException(input);
    }

    public static string getCodeBlock(ASTNode node)
    {
        return node.codeExcerpt;
    }

    public static string getCodeBlock(Util.Token token)
    {
        return token.value;
    }

    // public override string? StackTrace => "";

}
