using Spectre.Console;


public static class DebugConsole
{
    public static bool log = false;

    public static void Write(object obj)
    {
        if (log)
        {
            Console.WriteLine(obj);
        }
    }

    public static void Write()
    {
        if (log)
        {
            Console.WriteLine();
        }
    }


    public static void WriteAnsi(object obj)
    {
        if (log)
        {
            AnsiConsole.MarkupLine(obj.ToString());
        }

    }

}
