
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Program
{
    static void Main(string[] args)
    {
        Installations.linux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        Installations.windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        Installations.os = Installations.checkOs();


        Installations.resourcesUri = new Uri(@$"https://github.com/green726/HISS/releases/latest/download/Resources.zip");
        Installations.languageUri = new Uri(@$"https://github.com/green726/HISS/releases/latest/download/Language-{Installations.os}.zip");
        Installations.HIPUri = new Uri(@$"https://github.com/green726/HISS/releases/latest/download/HIP-{Installations.os}.zip");

        Installations.ps = Installations.windows ? @"\" : "/";


        InstallerOptions opts = CLI.parseOptions(args);

        if (opts.uninstall)
        {
            Installations.uninstall(opts.installPath);
            Console.WriteLine("HISS Uninstall successfull");
            return;
        }

        Console.WriteLine($"Chosen install path: {opts.installPath}");

        System.IO.Directory.CreateDirectory(opts.installPath);
        if (opts.installHIP)
        {
            Installations.installHIP(opts.installPath);
        }
        else
        {
            Installations.installLanguage(opts.installPath);
        }

        Console.WriteLine("Installation of HISS was successful - restart your terminal to use it | if you experience any issues restart your PC or make an issue on the HISS github repository");
    }

}

