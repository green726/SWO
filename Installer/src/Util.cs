using System.Runtime.InteropServices;
using System.IO.Compression;
using Spectre.Console;

public static class Util
{

    public static void figureOutSettings(Settings settings)
    {
        (string os, string ps) = checkOs();

        string path = settings.installPath;

        if (settings.uninstall)
        {
            uninstall(settings.installPath, @$"{Environment.GetEnvironmentVariable("HOME")}/.bashrc");
            Environment.Exit(0);
        }

        List<SWOComponent> components = new List<SWOComponent>();

        Directory.CreateDirectory(settings.installPath);


        string releaseVer = settings.version;
        if (settings.version == "stable")
        {
            releaseVer = "latest";
        }

        if (!settings.dontInstallResources)
        {
            components.Add(new SWOComponent($"https://github.com/green726/SWO/releases/{releaseVer}/download/Resources.zip", $"{path}{ps}Resources", $"{path}{ps}Resources.zip", "Installing SWO Resources", "Downloading SWO Resources", "Cloning SWO Resources", "Building SWO Resources", build: false));
        }
        if (!settings.dontInstallSAP)
        {
            components.Add(new SWOComponent($"https://github.com/green726/SAP/releases/{releaseVer}/download/SAP-{os}.zip", $"{path}{ps}SAP", $"{path}{ps}SAP.zip", "Installing", "Downloading SAP", "Cloning SAP", "Building SAP", "https://github.com/green726/SAP/SAP.git", "SAP"));
        }
        components.Add(new SWOComponent($"https://github.com/green726/SWO/releases/{releaseVer}/download/Language-{os}.zip", $"{path}{ps}Language", $"{path}{ps}Language.zip", "Installing the SWO Language", "Downloading the SWO Language", "Cloning SWO", "Building SWO", "https://github.com/green726/SWO/SWO.git", "SWO"));


        if (settings.version == "stable" || Double.TryParse(settings.version, out double version))
        {
            Installations.download(components).Wait();
            Installations.install(components).Wait();
            Installations.addToPath(settings);
        }
        else
        {
            Installations.clone(components, settings.version).Wait();
            Installations.build(components, os).Wait();
            Installations.addToPath(settings);
        }

    }


    public static void uninstall(string path, string bashrc)
    {
        try
        {
            Directory.Delete(path, true);
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("SWO install not found - continuing on to PATH variable removal");
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
            var newValue = oldValue.Replace(@$"{path}\Language\;", "").Replace(@$"{path}\SAP\;", "");
            Environment.SetEnvironmentVariable(envName, newValue, scope);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string hipEnv = "export PATH=$PATH:~/.SWO/SAP/";
            string langEnv = "export PATH=$PATH:~/.SWO/Language/";
            string text = File.ReadAllText(bashrc);
            text = text.Replace(hipEnv, "");
            text = text.Replace(langEnv, "");
            File.WriteAllText(bashrc, text);
        }
        Console.WriteLine("SWO Uninstalled Successfully");
    }

    public static string evaluatePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.SWO");
        }
        else
        {
            return $@"{Environment.GetEnvironmentVariable("HOME")}/.SWO";
        }

    }

    public static (string, string) checkOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Installations.linux = true;
            if (RuntimeInformation.OSArchitecture.ToString() == "X64")
            {
                return ("linux-x64", "/");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Installations.windows = true;
            return (RuntimeInformation.RuntimeIdentifier, @"\");
        }
        return ("unknown", "unknown");
    }


    public static Task extractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite, ref ProgressTask task)
    {
        int maxFiles = archive.Entries.Count();
        task.MaxValue = maxFiles;
        if (!overwrite)
        {
            archive.ExtractToDirectory(destinationDirectoryName);
            return Task.CompletedTask;
        }

        DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
        string destinationDirectoryFullPath = di.FullName;

        foreach (ZipArchiveEntry file in archive.Entries)
        {
            task.Increment(1);
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
