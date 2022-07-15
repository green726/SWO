
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Program
{
    static void Main(string[] args)
    {
        InstallerOptions opts = CLI.parseOptions(args);
        Console.WriteLine($"Chosen install path: {opts.installPath}");

        System.IO.Directory.CreateDirectory(opts.installPath);
        if (opts.installHIP)
        {
            Installations.installHIP(opts.installPath);
            Installations.installLanguage(opts.installPath);
        }
        else
        {
            Installations.installLanguage(opts.installPath);
        }

        Console.WriteLine("Installation of HISS was successful - restart your terminal to use it | if you experience any issues restart your PC or make an issue on the HISS github repository");
    }

}

