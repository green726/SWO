using System.Runtime.InteropServices;
using System.IO.Compression;

public static class Util
{

    public static async void figureOutSettings(Settings settings)
    {
        Console.WriteLine("Util figure out settings");
        (string os, string ps) = checkOs();

        string path = settings.installPath;

        if (settings.uninstall)
        {
            uninstall(settings.installPath, @$"{Environment.GetEnvironmentVariable("HOME")}/.bashrc");
            Environment.Exit(0);
        }

        List<HISSComponent> components = new List<HISSComponent>();

        Directory.CreateDirectory(settings.installPath);

        if (settings.installResources)
        {
            components.Add(new HISSComponent("https://github.com/green726/HISS/releases/latest/download/Resources.zip", $"{path}{ps}Resources", $"{path}{ps}Resources.zip", "Installing HISS Resources", "Downloading HISS Resources"));
        }
        if (settings.installHIP)
        {
            components.Add(new HISSComponent($"https://github.com/green726/HISS/releases/latest/download/HIP-{os}.zip", $"{path}{ps}HIP", $"{path}{ps}HIP.zip", "Install HIP (Highly Inefficient Packages) - The HISS Package Manager", "Downloading HIP (Highly Inefficient Packages) - the HISS Package Manager"));
        }
        components.Add(new HISSComponent($"https://github.com/green726/HISS/releases/latest/download/Language-{os}.zip", $"{path}{ps}Language", $"{path}{ps}Language.zip", "Installing the HISS Language", "Downloading the HISS Language"));

        await Installations.download(components);
        await Installations.install(components);
    }


    public static void uninstall(string path, string bashrc)
    {
        try
        {
            Directory.Delete(path, true);
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("HISS install not found - continuing on to PATH variable removal");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Please run as administrator");
            Environment.Exit(0);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string envName = "PATH";
            var scope = EnvironmentVariableTarget.Machine; // or User
            var oldValue = Environment.GetEnvironmentVariable(envName, scope);
            var newValue = oldValue.Replace(@$"{path}\Language\", "").Replace(@$"{path}\HIP\", "");
            Environment.SetEnvironmentVariable(envName, newValue, scope);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string hipEnv = "export PATH=$PATH:~/.HISS/HIP/";
            string langEnv = "export PATH=$PATH:~/.HISS/Language/";
            string text = File.ReadAllText(bashrc);
            text = text.Replace(hipEnv, "");
            text = text.Replace(langEnv, "");
            File.WriteAllText(bashrc, text);
        }

        Console.WriteLine("HISS Uninstalled Successfully");
    }

    public static string evaluatePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.HISS");
        }
        else
        {
            return $@"{Environment.GetEnvironmentVariable("HOME")}/.HISS";
        }

    }

    public static (string, string) checkOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (RuntimeInformation.OSArchitecture.ToString() == "X64")
            {
                return ("linux-x64", "/");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return (RuntimeInformation.RuntimeIdentifier, @"\");
        }
        return ("unknown", "unknown");
    }


    public static Task extractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite, ref int currentInstallFile)
    {
        if (!overwrite)
        {
            archive.ExtractToDirectory(destinationDirectoryName);
            return Task.CompletedTask;
        }

        DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
        string destinationDirectoryFullPath = di.FullName;

        foreach (ZipArchiveEntry file in archive.Entries)
        {
            currentInstallFile++;
            string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

            if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
            }

            if (file.Name == "")
            {// Assuming Empty for Directory
                Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                continue;
            }
            file.ExtractToFile(completeFileName, true);
        }

        return Task.CompletedTask;
    }
}
