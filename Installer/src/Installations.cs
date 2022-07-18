using System.Net;
using System.IO.Compression;
using System.Runtime.InteropServices;

public static class Installations
{
    public static WebClient client = new WebClient();

    public static string os = checkOs();


    public static Uri HIPUri = new Uri(@$"https://github.com/green726/HISS/releases/latest/download/HIP-{os}.zip");
    public static Uri languageUri = new Uri(@$"https://github.com/green726/HISS/releases/latest/download/Language-{os}.zip");
    public static Uri resourcesUri = new Uri(@$"https://github.com/green726/HISS/releases/latest/download/Resources.zip");

    public static bool windows = OperatingSystem.IsWindows();

    public static string ps = windows ? @"\" : "/"; //path seperator

    public static void installHIP(string path)
    {
        // System.IO.Directory.CreateDirectory(path + @"\HIP\");
        Console.WriteLine("pulling file from: " + HIPUri);
        client.DownloadFile(HIPUri, path + @$"{ps}HIP-{os}.zip");


        FileStream fs = new FileStream(path + @$"{ps}HIP-{os}.zip", FileMode.Open);
        ZipArchive archive = new ZipArchive(fs);
        extractToDirectory(archive, path + @$"{ps}HIP", true);

        if (windows)
        {
            string envName = "PATH";
            var scope = EnvironmentVariableTarget.Machine; // or User
            var oldValue = Environment.GetEnvironmentVariable(envName, scope);
            var newValue = oldValue + @$"{path}\HIP\;";
            Environment.SetEnvironmentVariable(envName, newValue, scope);
        }
        else if (OperatingSystem.IsLinux())
        {
            File.AppendAllText(@$"{Environment.GetEnvironmentVariable("HOME")}/.bashrc", "export PATH=$PATH:~/.HISS/HIP/");
        }

        fs.Dispose();
        installLanguage(path);
    }

    public static void installLanguage(string path)
    {
        // System.IO.Directory.CreateDirectory(path + @"\Language\");
        client.DownloadFile(languageUri, path + @$"{ps}Language-{os}.zip");


        FileStream fs = new FileStream(path + $@"{ps}Language-{os}.zip", FileMode.Open);
        ZipArchive archive = new ZipArchive(fs);
        extractToDirectory(archive, path + @$"{ps}Language", true);

        if (windows)
        {
            string envName = "PATH";
            var scope = EnvironmentVariableTarget.Machine; // or User
            var oldValue = Environment.GetEnvironmentVariable(envName, scope);
            var newValue = oldValue + @$"{path}\Language\;";
            Environment.SetEnvironmentVariable(envName, newValue, scope);
        }
        else if (OperatingSystem.IsLinux())
        {
            File.AppendAllText(@$"{Environment.GetEnvironmentVariable("HOME")}/.bashrc", "export PATH=$PATH:~/.HISS/Language/");
        }


        fs.Dispose();

        installResources(path);

    }

    public static void uninstall(string path)
    {
        Directory.Delete(path);
    }

    public static void installResources(string path)
    {
        client.DownloadFile(resourcesUri, path + @$"{ps}Resources.zip");

        FileStream fs = new FileStream(path + @$"{ps}Resources.zip", FileMode.Open);
        ZipArchive archive = new ZipArchive(fs);
        extractToDirectory(archive, path + @$"{ps}Resources", true);
    }

    public static string checkOs()
    {
        if (OperatingSystem.IsLinux())
        {
            if (RuntimeInformation.OSArchitecture.ToString() == "X64")
            {
                return "linux-x64";
            }
        }
        else if (OperatingSystem.IsWindows())
        {
            return RuntimeInformation.RuntimeIdentifier;
        }
        return "";
    }

    public static void extractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
    {
        Console.WriteLine($"input dest direc name: {destinationDirectoryName}");
        if (!overwrite)
        {
            archive.ExtractToDirectory(destinationDirectoryName);
            return;
        }

        DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
        string destinationDirectoryFullPath = di.FullName;

        foreach (ZipArchiveEntry file in archive.Entries)
        {
            Console.WriteLine($"dest direc: {destinationDirectoryFullPath}");
            Console.WriteLine($"file name: {file.FullName}");
            Console.WriteLine(Path.Combine(destinationDirectoryFullPath, file.FullName));
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
    }
}
