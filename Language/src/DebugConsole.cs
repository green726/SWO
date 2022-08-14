using Spectre.Console;


using LLVMSharp;


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

    public static void DumpType(LLVMTypeRef type)
    {
        if (log)
        {
            LLVM.DumpType(type);
        }
    }

    public static void DumpValue(LLVMValueRef value)
    {
        if (log)
        {
            LLVM.DumpValue(value);
        }
    }

    public static void DumpModule(LLVMModuleRef module)
    {
        if (log)
        {
            LLVM.DumpModule(module);
        }
    }

}
