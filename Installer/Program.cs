
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Program
{
    static void Main(string[] args)
    {
        Installations.init();

        CLI cli = new CLI();
        if (args.Length > 0 && args[0] == "ui")
        {
            Prompt.getOptions();
        }


        Console.WriteLine("Installation of HISS was successful - restart your terminal to use it | if you experience any issues restart your PC or make an issue on the HISS github repository");
    }

}

