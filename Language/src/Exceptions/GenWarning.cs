using Spectre.Console;

public static class GenWarning
{
    public static void write(string message)
    {
        message = @$"[yellow]{message}[/]";
        DebugConsole.WriteAnsi(message);
    }


    public static void write(string message, AST.Node node)
    {
        message = $"{message} at [blue]{node.line}:{node.column}[/]:\n```\n[purple]{node.codeExcerpt}[/]\n```";
        DebugConsole.WriteAnsi(message);
    }
}
