
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

}
