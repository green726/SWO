using System;
using System.Collections;
using System.Runtime.Serialization;

[Serializable]
public class ParserException : Exception
{
    public override IDictionary Data => base.Data;

    public override string? HelpLink { get => base.HelpLink; set => base.HelpLink = value; }

    public override string Message => base.Message;

    public override string? Source { get => base.Source; set => base.Source = value; }

    public override string? StackTrace { get; }/* => base.StackTrace; */

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

    public static ParserException FactoryMethod(string whatHappened, string recommendedFix, Util.Token token, AST.Node? parent = null, bool typoSuspected = false)
    {
        string input = "";
        string codeBlock = "";
        if (parent != null)
        {
            codeBlock = $"{getCodeBlock(parent)} \n--------\n{getCodeBlock(token)}";
        }
        else
        {
            codeBlock = getCodeBlock(token);
        }
        if (typoSuspected && Config.settings.general.typo.enabled)
        {
            List<string> typoFixes = Typo.spellCheck(token.value);
            input = $"{whatHappened}: \n```\n{codeBlock}\n```\nHow You Can Fix This: \n{recommendedFix} \nPossible typo solutions: {typoFixes.ToString()}\nError Was Thrown At {token.line}:{token.column}";
        }
        else
        {
            input = $"{whatHappened}: \n```\n{codeBlock}\n```\nHow You Can Fix This: \n{recommendedFix} \nError Was Thrown At {token.line}:{token.column}";
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
            input = $"{whatHappened}: \n```\n{codeBlock}\n```\nHow You Can Fix This: \n{recommendedFix} \nPossible typo solutions: {typoFixes.ToString()}\nError Was Thrown At {node.line}:{node.column}";
        }
        else
        {
            string codeBlock = getCodeBlock(node);
            input = $"{whatHappened}\n```\n{codeBlock}\n```\nHow You Can Fix This: \n{recommendedFix} \nError Was Thrown At {node.line}:{node.column}";
        }

        return new ParserException(input);
    }

    public static ParserException FactoryMethod(string whatHappened, string recommendedFix, AST.Node node, AST.Node parent, bool typoSuspected = false, string typoString = "")
    {
        string input = "";
        if (typoSuspected && Config.settings.general.typo.enabled)
        {
            string codeBlock = getCodeBlock(node);
            List<string> typoFixes = Typo.spellCheck(typoString);
            input = $"{whatHappened}: \n```\n{codeBlock}\n```\nHow You Can Fix This: \n{recommendedFix} \nPossible typo solutions: {typoFixes.ToString()}\nError Was Thrown At {node.line}:{node.column}";
        }
        else
        {
            string codeBlock = getCodeBlock(node);
            input = $"{whatHappened}\n```\n{codeBlock}\n```\nHow You Can Fix This: \n{recommendedFix} \nError Was Thrown At {node.line}:{node.column}";
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

}
