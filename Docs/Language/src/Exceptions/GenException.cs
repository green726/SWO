using Spectre.Console;

[Serializable]
public class GenException : Exception
{

    public override string? StackTrace { get; }/* => base.StackTrace; */
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
        if (typoSuspected && Config.settings.general.typo.enabled)
        {
            string codeBlock = getCodeBlock(node);
            List<string> typoFixes = Typo.spellCheck(typoString);
            input = $"{whatHappened}: \n```\n{codeBlock}\n```\nHow You Can Fix This: \n{recommendedFix} \nPossible typo solutions: {string.Join(", ", typoFixes)}\nError Was Thrown At {node.line}:{node.column}";
        }
        else
        {
            string codeBlock = getCodeBlock(node);
            input = $"{whatHappened}: \n```\n{codeBlock}\n```\nHow You Can Fix This: \n{recommendedFix} \nError Was Thrown At {node.line}:{node.column}";
        }

        // AnsiConsole.WriteException(new GenException(input));
        // throw new Exception();
        return new GenException(input);
    }

    public static string getCodeBlock(AST.Node node)
    {
        return node.codeExcerpt;
    }
}
