using Spectre.Console;

public static class GenWarning
{
    public static void write(string message, AST.Node? node = null)
    {
        message = @$"[yellow]{message}[\]";
        if (node != null)
        {
            message = $"{message} at [blue]{node.line}:{node.column}[\\]:\n {node.codeExcerpt}";
        }
        Spectre.Console.AnsiConsole.WriteLine(message);
    }
}
