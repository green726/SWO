using System;

public class ParserException : Exception
{
    public ParserException(string msg, AST.Node node) : base($"{msg} at {node?.line}:{node?.column}")
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

    public static ParserException FactoryMethod(string whatHappened, string recommendedFix, Util.Token token, bool typoSuspected = false)
    {
        string input = "";
        if (typoSuspected && Config.settings.general.typo.enabled)
        {
            string codeBlock = getCodeBlock(token);
            List<string> typoFixes = Typo.spellCheck(token.value);
            input = $"{whatHappened} - ```\n{codeBlock}```, \n How You Can Fix This: \n {recommendedFix} \n Possible typo solutions: {typoFixes.ToString()}\n Error Was Thrown At {token.line}:{token.column}";
        }
        else
        {
            string codeBlock = getCodeBlock(token);
            input = $"{whatHappened} - ```\n{codeBlock}```, \n How You Can Fix This: \n {recommendedFix} \n Error Was Thrown At {token.line}:{token.column}";
        }

        return new ParserException(input);
    }

    public static ParserException FactoryMethod(string whatHappened, string recommendedFix, AST.Node node, bool typoSuspected = false, string typoString = "")
    {
        string input = "";
        if (typoSuspected && Config.settings.general.typo.enabled)
        {
            string codeBlock = getCodeBlock(node);
            List<string> typoFixes = Typo.spellCheck(typoString);
            input = $"{whatHappened} - ```\n{codeBlock}```, \n How You Can Fix This: \n {recommendedFix} \n Possible typo solutions: {typoFixes.ToString()}\n Error Was Thrown At {node.line}:{node.column}";
        }
        else
        {
            string codeBlock = getCodeBlock(node);
            input = $"{whatHappened} - ```\n{codeBlock}```, \n How You Can Fix This: \n {recommendedFix} \n Error Was Thrown At {node.line}:{node.column}";
        }

        return new ParserException(input);
    }

    public static string getCodeBlock(AST.Node node)
    {
        return node.codeExcerpt;
    }

    public static string getCodeBlock(Util.Token token)
    {
        return token.value;
    }

    // public override string? StackTrace => "";

}
