using LLVMSharp;

public static class EXE
{
    public static IntPtr targetTriple = LLVM.GetDefaultTargetTriple();
    public static LLVMTargetRef target;
    public static LLVMBool targetBool;
    public static string errorMsg = "EEE";

    public static void compileEXE()
    {
        LLVM.InitializeAllTargetInfos();
        LLVM.InitializeAllTargets();
        LLVM.InitializeAllTargetMCs();
        LLVM.InitializeAllAsmParsers();
        LLVM.InitializeAllAsmPrinters();

        targetBool = LLVM.GetTargetFromTriple(targetTriple.ToString(), out target, out errorMsg);

        Console.WriteLine("targetBool" + targetBool.Value);


    }
}
