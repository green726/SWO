using LLVMSharp;
using System.Runtime.InteropServices;
using System.Diagnostics;

public static class EXE
{
    public static string? targetTriple;
    public static LLVMTargetRef target;
    public static LLVMTargetMachineRef targetMachine;
    public static LLVMBool targetBool;
    public static LLVMPassManagerRef passManager = new LLVMPassManagerRef();
    public static LLVMMemoryBufferRef memBuffer = new LLVMMemoryBufferRef();


    public static string targetErrorMsg = "";
    public static string writeErrorMsg = "";

    public static void compileEXE(bool windows, string fileName = "output", bool debugLogging = true)
    {
        LLVM.InitializeX86TargetInfo();
        LLVM.InitializeX86Target();
        LLVM.InitializeX86TargetMC();
        LLVM.InitializeX86AsmParser();
        LLVM.InitializeX86AsmPrinter();

        IntPtr fileNamePtr;
        // Marshal.FreeHGlobal(fileNamePtr); //BUG: this line breaks the code, maybe we are supposed to do it after we use the fileNamePtr?


        string? fileNameFinal;
        if (windows)
        {
            targetTriple = Marshal.PtrToStringUTF8(LLVM.GetDefaultTargetTriple());
            fileNamePtr = Marshal.StringToHGlobalAnsi(fileName + ".o");
            fileNameFinal = Marshal.PtrToStringAnsi(fileNamePtr);
        }
        else
        {
            targetTriple = Marshal.PtrToStringAuto(LLVM.GetDefaultTargetTriple());
            fileNamePtr = Marshal.StringToHGlobalAuto(fileName + ".o");
            fileNameFinal = Marshal.PtrToStringAuto(fileNamePtr);
        }

        targetBool = LLVM.GetTargetFromTriple(targetTriple, out target, out targetErrorMsg);
        targetMachine = LLVM.CreateTargetMachine(target, targetTriple, "generic", "", LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);


        // LLVM.SetTarget(IRGen.module, targetTriple); //idk what these lines of code do, but they didn't fix targetmachine cant generate filetype error
        // LLVMTargetDataRef dataRef = LLVM.CreateTargetDataLayout(targetMachine);
        // string? dataRefString = Marshal.PtrToStringAuto(LLVM.CopyStringRepOfTargetData(dataRef));
        // LLVM.SetDataLayout(IRGen.module, dataRefString);

        if (debugLogging)
        {
            Console.WriteLine("beggining of object file debug info");

            Console.WriteLine("fileNamePostPtr " + fileNameFinal);
            Console.WriteLine("TargetTriple:" + targetTriple);
            Console.WriteLine("targetBool: " + targetBool.Value);
            Console.WriteLine("targetRef: " + target.ToString());
            Console.WriteLine("targetMachine: " + targetMachine.ToString());

            if (targetErrorMsg != null && targetErrorMsg != "")
            {
                Console.WriteLine("targetErrorMsg: " + targetErrorMsg);
            }
            if (windows)
            {
                Console.WriteLine("IR writing for windows (doesn't work so doesn't do it)");
                /* moduleStr =  */
                // LLVM.PrintModuleToFile(IRGen.module, $"{fileName}-IR", out string errorMsg);
            }
            else
            {
                string moduleStr = Marshal.PtrToStringAuto(LLVM.PrintModuleToString(IRGen.module));
                File.WriteAllText($"{fileName}-IR", moduleStr);
            }
        }

        // LLVM.TargetMachineEmitToMemoryBuffer(targetMachine, IRGen.module, LLVMCodeGenFileType.LLVMObjectFile, out writeErrorMsg, out memBuffer);
        if (windows)
        {
            LLVM.TargetMachineEmitToFile(targetMachine, IRGen.module, fileNamePtr, LLVMCodeGenFileType.LLVMObjectFile, out writeErrorMsg);
        }
        else
        {
            LLVM.TargetMachineEmitToFile(targetMachine, IRGen.module, fileNamePtr, LLVMCodeGenFileType.LLVMObjectFile, out writeErrorMsg);
        }
        if (writeErrorMsg != null && writeErrorMsg != "" && debugLogging)
        {
            Console.WriteLine("writeErrorMsg: " + writeErrorMsg);
        }
    }


    public static void compileEXE(CompileCommandSettings settings, bool debugLogging = false)
    {
        bool windows = settings.targetOSName == "win10-x64";

        LLVM.InitializeX86TargetInfo();
        LLVM.InitializeX86Target();
        LLVM.InitializeX86TargetMC();
        LLVM.InitializeX86AsmParser();
        LLVM.InitializeX86AsmPrinter();
        IntPtr fileNamePtr;
        // Marshal.FreeHGlobal(fileNamePtr); //BUG: this line breaks the code, maybe we are supposed to do it after we use the fileNamePtr?


        string? fileNameFinal;

        if (windows)
        {
            targetTriple = Marshal.PtrToStringUTF8(LLVM.GetDefaultTargetTriple());
            fileNamePtr = Marshal.StringToHGlobalAnsi(settings.resultFileName + ".o");
            fileNameFinal = Marshal.PtrToStringAnsi(fileNamePtr);
        }
        else
        {
            targetTriple = Marshal.PtrToStringAuto(LLVM.GetDefaultTargetTriple());
            fileNamePtr = Marshal.StringToHGlobalAuto(settings.resultFileName + ".o");
            fileNameFinal = Marshal.PtrToStringAuto(fileNamePtr);
        }

        targetBool = LLVM.GetTargetFromTriple(targetTriple, out target, out targetErrorMsg);
        targetMachine = LLVM.CreateTargetMachine(target, targetTriple, "generic", "", LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);


        // LLVM.SetTarget(IRGen.module, targetTriple); //idk what these lines of code do, but they didn't fix targetmachine cant generate filetype error
        // LLVMTargetDataRef dataRef = LLVM.CreateTargetDataLayout(targetMachine);
        // string? dataRefString = Marshal.PtrToStringAuto(LLVM.CopyStringRepOfTargetData(dataRef));
        // LLVM.SetDataLayout(IRGen.module, dataRefString);

        if (settings.resultFileType == FileType.LLVMIR)
        {
            LLVM.PrintModuleToFile(IRGen.module, $"{settings.resultFileName}.ir", out string errorMsg);
        }

        else if (settings.resultFileType == FileType.NativeExecutable || settings.resultFileType == FileType.Object || settings.resultFileType == FileType.Binary)
        {
            LLVM.TargetMachineEmitToFile(targetMachine, IRGen.module, fileNamePtr, LLVMCodeGenFileType.LLVMObjectFile, out writeErrorMsg);
        }
        else if (settings.resultFileType == FileType.Assembly)
        {
            LLVM.TargetMachineEmitToFile(targetMachine, IRGen.module, fileNamePtr, LLVMCodeGenFileType.LLVMAssemblyFile, out writeErrorMsg);
        }
        if (writeErrorMsg != null && writeErrorMsg != "" && debugLogging)
        {
            Console.WriteLine("writeErrorMsg: " + writeErrorMsg);
        }

        if (settings.resultFileType == FileType.NativeExecutable)
        {
            link(settings);
        }

    }

    public static void link(CompileCommandSettings settings)
    {
        string fullObjectPath = Path.GetFullPath(@$"{settings.path}/{settings.resultFileName}.o");

        Process process = new Process();

        process.StartInfo.FileName = Config.settings.general.linker.path;
        process.StartInfo.Arguments = $"{Config.settings.general.linker.args} {settings.resultFileName} {fullObjectPath}";

        process.Start();

        process.WaitForExit();

        string executableEnding = settings.targetOSName == "win10-x64" ? ".exe" : ".out";
        string exeFile = Path.GetFullPath(settings.path + "/a" + executableEnding);

        File.Delete(fullObjectPath);
    }
}
