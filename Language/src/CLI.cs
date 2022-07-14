using System.Runtime.InteropServices;
using System.ComponentModel;

public static class CLI
{
    public static CompilerOptions parseOptions(string[] inputs)
    {
        CompilerOptions compilerOptions = new CompilerOptions();
        string currentOption = "";
        int index = 0;
        foreach (string input in inputs)
        {
            if (currentOption != "")
            {
                compilerOptions.GetType().GetProperty(currentOption).SetValue(compilerOptions, input);
                currentOption = "";
            }
            else if (input.StartsWith("--"))
            {
                currentOption = input.Substring(2);
                Console.WriteLine("current option changed to " + currentOption);
            }
            else
            {
                if (index > 1)
                {
                    throw new Exception("illegal compiler option");
                }
                Console.WriteLine("set compiler options options with index of " + index + " to input of " + input);
                compilerOptions.GetType().GetProperty(compilerOptions.options[index]).SetValue(compilerOptions, input);
            }
            index++;
        }

        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(compilerOptions))
        {
            string name = descriptor.Name;
            object value = descriptor.GetValue(compilerOptions);
            Console.WriteLine($"compiler option: {name} with a value of {value}");
        }

        throw new Exception();
        return compilerOptions;
    }
}

public enum FileType
{
    Assembly,
    Object,
    Binary,
    LLVMIR,
    NativeExecutable,
}

public class CompilerOptions
{
    public string file { get; set; } = "";
    public string folder { get; set; } = "";
    public string configFile { get; set; } = "";

    public FileType targetFileType { get; set; } = FileType.NativeExecutable;
    public string targetFileName { get; set; } = "my-hiss-program";
    public string targetOSName { get; set; } = "";

    public string[] options = new string[] { "file", "targetFileName" };

    public CompilerOptions()
    {
        this.targetOSName = RuntimeInformation.RuntimeIdentifier;
    }

}
