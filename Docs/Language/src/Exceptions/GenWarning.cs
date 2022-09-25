using Spectre.Console;

public static class GenWarning
{
    public static void write(string message, AST.Node? node = null)
    {
        message = @$"[yellow]{message}[/]";
        if (node != null)
        {
            message = $"{message} at [blue]{node.line}:{node.column}[/]:\n```\n[purple]{node.codeExcerpt}[/]\n```";
        }
        DebugConsole.WriteAnsi(message);
    }
}
