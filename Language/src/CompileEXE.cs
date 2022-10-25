using LLVMSharp;
using System.Runtime.InteropServices;
using System.Diagnostics;

public static class EXE
{
    public static string targetTriple;
    public static LLVMTargetRef target;
    public static LLVMTargetMachineRef targetMachine;
    public static LLVMBool targetBool;
    public static LLVMPassManagerRef passManager = new LLVMPassManagerRef();
    public static LLVMMemoryBufferRef memBuffer = new LLVMMemoryBufferRef();

    public static string targetErrorMsg = "";
    public static string writeErrorMsg = "";


    public static string compileEXE(CompileCommandSettings settings, IRGen generator)
    {
        bool windows = settings.targetOSName == "win10-x64";

        LLVM.InitializeX86TargetInfo();
        LLVM.InitializeX86Target();
        LLVM.InitializeX86TargetMC();
        LLVM.InitializeX86AsmParser();
        LLVM.InitializeX86AsmPrinter();


        IntPtr fileNamePtr;
        // Marshal.FreeHGlobal(fileNamePtr); //BUG: this line breaks the code, maybe we are supposed to do it after we use the fileNamePtr?

        string fileEnding = ".o";

        switch (settings.resultFileType)
        {
            case FileType.Assembly:
                fileEnding = ".asm";
                break;
            case FileType.Object:
                break;
            case FileType.NativeExecutable:
                break;
        }

        string fileNameFinal = "";

        if (windows)
        {
            targetTriple = Marshal.PtrToStringUTF8(LLVM.GetDefaultTargetTriple());
            fileNamePtr = Marshal.StringToHGlobalAnsi(generator.fileName + fileEnding);
            fileNameFinal = Marshal.PtrToStringAnsi(fileNamePtr);
        }
        else
        {
            targetTriple = Marshal.PtrToStringAuto(LLVM.GetDefaultTargetTriple());
            fileNamePtr = Marshal.StringToHGlobalAuto(generator.fileName + fileEnding);
            fileNameFinal = Marshal.PtrToStringAuto(fileNamePtr);
        }

        targetBool = LLVM.GetTargetFromTriple(targetTriple, out target, out targetErrorMsg);
        targetMachine = LLVM.CreateTargetMachine(target, targetTriple, "generic", "", LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);


        // LLVM.SetTarget(generator.module, targetTriple); //idk what these lines of code do, but they didn't fix targetmachine cant generate filetype error
        // LLVMTargetDataRef dataRef = LLVM.CreateTargetDataLayout(targetMachine);
        // string? dataRefString = Marshal.PtrToStringAuto(LLVM.CopyStringRepOfTargetData(dataRef));
        // LLVM.SetDataLayout(generator.module, dataRefString);

        if (settings.resultFileType == FileType.LLVMIR)
        {
            LLVM.PrintModuleToFile(generator.module, $"{generator.fileName}.ir", out string errorMsg);
        }

        else if (settings.resultFileType == FileType.NativeExecutable || settings.resultFileType == FileType.Object || settings.resultFileType == FileType.Binary)
        {
            LLVM.TargetMachineEmitToFile(targetMachine, generator.module, fileNamePtr, LLVMCodeGenFileType.LLVMObjectFile, out writeErrorMsg);
        }
        else if (settings.resultFileType == FileType.Assembly)
        {
            LLVM.TargetMachineEmitToFile(targetMachine, generator.module, fileNamePtr, LLVMCodeGenFileType.LLVMAssemblyFile, out writeErrorMsg);
        }
        if (writeErrorMsg != null && writeErrorMsg != "")
        {
            DebugConsole.Write("writeErrorMsg: " + writeErrorMsg);
        }
        return generator.fileName;
    }

    public static string compileEXE(CompileCommandSettings settings, IRGen generator, Spectre.Console.ProgressTask task)
    {
        task.MaxValue = 10;

        bool windows = settings.targetOSName == "win10-x64";

        LLVM.InitializeX86TargetInfo();
        LLVM.InitializeX86Target();
        LLVM.InitializeX86TargetMC();
        LLVM.InitializeX86AsmParser();
        LLVM.InitializeX86AsmPrinter();

        task.Increment(3);

        IntPtr fileNamePtr;
        // Marshal.FreeHGlobal(fileNamePtr); //BUG: this line breaks the code, maybe we are supposed to do it after we use the fileNamePtr?

        string fileEnding = ".o";

        switch (settings.resultFileType)
        {
            case FileType.Assembly:
                fileEnding = ".asm";
                break;
            case FileType.Object:
                break;
            case FileType.NativeExecutable:
                break;
        }

        string fileNameFinal = "";

        if (windows)
        {
            targetTriple = Marshal.PtrToStringUTF8(LLVM.GetDefaultTargetTriple());
            fileNamePtr = Marshal.StringToHGlobalAnsi(generator.fileName + fileEnding);
            fileNameFinal = Marshal.PtrToStringAnsi(fileNamePtr);
        }
        else
        {
            targetTriple = Marshal.PtrToStringAuto(LLVM.GetDefaultTargetTriple());
            fileNamePtr = Marshal.StringToHGlobalAuto(generator.fileName + fileEnding);
            fileNameFinal = Marshal.PtrToStringAuto(fileNamePtr);
        }

        targetBool = LLVM.GetTargetFromTriple(targetTriple, out target, out targetErrorMsg);
        targetMachine = LLVM.CreateTargetMachine(target, targetTriple, "generic", "", LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);


        // LLVM.SetTarget(generator.module, targetTriple); //idk what these lines of code do, but they didn't fix targetmachine cant generate filetype error
        // LLVMTargetDataRef dataRef = LLVM.CreateTargetDataLayout(targetMachine);
        // string? dataRefString = Marshal.PtrToStringAuto(LLVM.CopyStringRepOfTargetData(dataRef));
        // LLVM.SetDataLayout(generator.module, dataRefString);

        if (settings.resultFileType == FileType.LLVMIR)
        {
            LLVM.PrintModuleToFile(generator.module, $"{generator.fileName}.ir", out string errorMsg);
        }

        else if (settings.resultFileType == FileType.NativeExecutable || settings.resultFileType == FileType.Object || settings.resultFileType == FileType.Binary)
        {
            LLVM.TargetMachineEmitToFile(targetMachine, generator.module, fileNamePtr, LLVMCodeGenFileType.LLVMObjectFile, out writeErrorMsg);
        }
        else if (settings.resultFileType == FileType.Assembly)
        {
            LLVM.TargetMachineEmitToFile(targetMachine, generator.module, fileNamePtr, LLVMCodeGenFileType.LLVMAssemblyFile, out writeErrorMsg);
        }
        task.Increment(4);
        if (writeErrorMsg != null && writeErrorMsg != "")
        {
            DebugConsole.Write("writeErrorMsg: " + writeErrorMsg);
        }

        task.StopTask();

        return generator.fileName;
    }

    public static void link(CompileCommandSettings settings, List<string> fileNamesList)
    {
        string fullObjectPath = Path.GetFullPath(@$"{settings.path}/");

        foreach (string name in fileNamesList)
        {
            fullObjectPath += name + ".o" + " ";
            DebugConsole.Write(name);
        }

        Process process = new Process();

        process.StartInfo.FileName = Config.settings.general.linker.path;
        process.StartInfo.Arguments = $"{Config.settings.general.linker.args} {settings.resultFileName} {fullObjectPath}";

        process.Start();

        process.WaitForExit();

        string executableEnding = settings.targetOSName == "win10-x64" ? ".exe" : ".out";
        string exeFile = Path.GetFullPath(settings.path + "/a" + executableEnding);

        foreach (string fileToDeleteName in fileNamesList)
        {
            File.Delete(Path.GetFullPath(@$"{settings.path}/{fileToDeleteName}.o"));
        }

    }
}
