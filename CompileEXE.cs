using LLVMSharp;
using System.Runtime.InteropServices;

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

    public static void compileEXE(string fileName = "output.o")
    {
        LLVM.InitializeX86TargetInfo();
        LLVM.InitializeX86Target();
        LLVM.InitializeX86TargetMC();
        LLVM.InitializeX86AsmParser();
        LLVM.InitializeX86AsmPrinter();

        IntPtr fileNamePtr = Marshal.StringToHGlobalAuto(fileName);
        // Marshal.FreeHGlobal(fileNamePtr); //BUG: this line breaks the code, maybe we are supposed to do it after we use the fileNamePtr?

        string? fileNameFinal = Marshal.PtrToStringAuto(fileNamePtr);
        Console.WriteLine("fileNamePostPtr " + fileNameFinal);

        targetTriple = Marshal.PtrToStringAuto(LLVM.GetDefaultTargetTriple());

        targetBool = LLVM.GetTargetFromTriple(targetTriple, out target, out targetErrorMsg);
        targetMachine = LLVM.CreateTargetMachine(target, targetTriple, "generic", "", LLVMCodeGenOptLevel.LLVMCodeGenLevelDefault, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);


        // LLVM.SetTarget(IRGen.module, targetTriple); //idk what these lines of code do, but they didn't fix targetmachine cant generate filetype error
        // LLVMTargetDataRef dataRef = LLVM.CreateTargetDataLayout(targetMachine);
        // string? dataRefString = Marshal.PtrToStringAuto(LLVM.CopyStringRepOfTargetData(dataRef));
        // LLVM.SetDataLayout(IRGen.module, dataRefString);

        Console.WriteLine("TargetTriple:" + targetTriple);
        Console.WriteLine("targetBool " + targetBool.Value);
        Console.WriteLine("targetRef " + target.ToString());
        Console.WriteLine("targetMachine " + targetMachine.ToString());
        Console.WriteLine("targetErrorMsg " + targetErrorMsg);

        // LLVM.TargetMachineEmitToMemoryBuffer(targetMachine, IRGen.module, LLVMCodeGenFileType.LLVMObjectFile, out writeErrorMsg, out memBuffer);
        LLVM.TargetMachineEmitToFile(targetMachine, IRGen.module, fileNamePtr, LLVMCodeGenFileType.LLVMObjectFile, out writeErrorMsg);

        Console.WriteLine("writeErrorMsg " + writeErrorMsg);



    }
}
