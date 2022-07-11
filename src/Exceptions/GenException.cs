using System;

public class GenException : Exception
{
    public GenException(string msg, AST.Node node) : base($"{msg} at {node.line}:{node.column}")
    {
    }

    public GenException(string msg, int line, int column) : base($"{msg} at {line}:{column}")
    {
    }

    public GenException(string msg) : base(msg) { }

    public static GenException FactoryMethod(string whatHappened, string recommendedFix, AST.Node node, bool typoSuspected = false, string typoString = "")
    {
        string input = "";
        if (typoSuspected)
        {
            string codeBlock = getCodeBlock(node);
            List<string> typoFixes = Typo.spellCheck(typoString);
            input = $"{whatHappened} - ```\n{codeBlock}```, \n How You Can Fix This: \n {recommendedFix} \n Possible typo solutions: {string.Join(", ", typoFixes)}\n Error Was Thrown At {node.line}:{node.column}";
        }
        else
        {
            string codeBlock = getCodeBlock(node);
            input = $"{whatHappened} - ```\n{codeBlock}```, \n How You Can Fix This: \n {recommendedFix} \n Error Was Thrown At {node.line}:{node.column}";
        }

        return new GenException(input);
    }

    public static string getCodeBlock(AST.Node node)
    {
        return "";
    }

    // public override string? StackTrace => "";
}
