using System.Runtime.InteropServices;
using System.ComponentModel;

public static class CLI
{
    public static InstallerOptions parseOptions(string[] inputs)
    {
        InstallerOptions installerOptions = new InstallerOptions();
        string currentOption = "";
        int index = 0;
        bool inputIsBool = false;
        bool inputBool = false;

        foreach (string input in inputs)
        {
            if (input == "true")
            {
                inputIsBool = true;
                inputBool = true;
            }
            else if (input == "false")
            {
                inputIsBool = true;
                inputBool = false;
            }
            else
            {
                inputIsBool = false;
            }
            if (currentOption != "")
            {
                if (inputIsBool)
                {
                    installerOptions.GetType().GetProperty(currentOption).SetValue(installerOptions, inputBool);
                }
                else
                {
                    installerOptions.GetType().GetProperty(currentOption).SetValue(installerOptions, input);
                }
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
                    throw new Exception("Unknown or illegal installer argument");
                }
                Console.WriteLine("set installer options options with index of " + index + " to input of " + input);
                if (inputIsBool)
                {
                    installerOptions.GetType().GetProperty(installerOptions.options[index]).SetValue(installerOptions, inputBool);
                }
                else
                {
                    installerOptions.GetType().GetProperty(installerOptions.options[index]).SetValue(installerOptions, input);
                }
            }
            index++;
        }

        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(installerOptions))
        {
            string name = descriptor.Name;
            object value = descriptor.GetValue(installerOptions);
            Console.WriteLine($"compiler option: {name} with a value of {value}");
        }

        return installerOptions;
    }
}

// public enum FileType
// {
//     Assembly,
//     Object,
//     Binary,
//     LLVMIR,
//     NativeExecutable,
// }

public class InstallerOptions
{
    public bool installHIP = true;
    public string installPath = "";

    public string[] options = new string[] { "installPath", "installHIP" };

    public InstallerOptions()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            installPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.HISS");
        }
        else
        {
            installPath = $@"{Environment.GetEnvironmentVariable("HOME")}/.HISS";
        }
    }

}
