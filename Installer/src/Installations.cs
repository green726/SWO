using System.Net;
using System.IO.Compression;
using System.Runtime.InteropServices;

public static class Installations
{


    public static string os;


    public static Uri? HIPUri;
    public static Uri? languageUri;
    public static Uri? resourcesUri;

    public static bool windows;
    public static bool linux;

    public static string ps; //path seperator

    public static string bashrc = @$"{Environment.GetEnvironmentVariable("HOME")}/.bashrc";

    public static ProgressBar HIPBar;
    public static ProgressBar langBar;
    public static ProgressBar resourcesBar;

    public static void HIPProgressChanged(object sender, DownloadProgressChangedEventArgs arg)
    {
        HIPBar.Report(arg.ProgressPercentage / 100);

    }
    public static void languageProgressChanged(object sender, DownloadProgressChangedEventArgs arg)
    {
        langBar.Report(arg.ProgressPercentage / 100);

    }
    public static void resourcesProgressChanged(object sender, DownloadProgressChangedEventArgs arg)
    {
        resourcesBar.Report(arg.ProgressPercentage / 100);

    }


    public static async void installHIP(string path)
    {
        // System.IO.Directory.CreateDirectory(path + @"\HIP\");
        // Console.WriteLine("pulling file from: " + HIPUri);


        WebClient client = new WebClient();

        HIPBar = new ProgressBar();
        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(HIPProgressChanged);

        Task HIPTask = client.DownloadFileTaskAsync(HIPUri, path + @$"{ps}HIP-{os}.zip");

        installLanguage(path);

        HIPTask.Wait();

        Console.WriteLine("HIP Install complete");

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
        else if (linux)
        {
            File.AppendAllText(bashrc, "export PATH=$PATH:~/.HISS/HIP/ \n");
        }

        fs.Dispose();
        // installLanguage(path);
    }

    public static async void installLanguage(string path)
    {
        // System.IO.Directory.CreateDirectory(path + @"\Language\");

        langBar = new ProgressBar();
        WebClient client = new WebClient();

        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(languageProgressChanged);
        Task langTask = client.DownloadFileTaskAsync(languageUri, path + @$"{ps}Language-{os}.zip");

        installResources(path);

        langTask.Wait();

        Console.WriteLine("HISS Language install complete");

        FileStream fs = new FileStream(path + $@"{ps}Language-{os}.zip", FileMode.Open);
        ZipArchive archive = new ZipArchive(fs);
        extractToDirectory(archive, path + @$"{ps}Language", true);

        if (windows)
        {
            string envName = "PATH";
            var scope = EnvironmentVariableTarget.Machine; // or User
            var oldValue = Environment.GetEnvironmentVariable(envName, scope);
            Console.WriteLine($"OLD PATH: {oldValue}");
            var newValue = oldValue + @$"{path}\Language\;";
            Environment.SetEnvironmentVariable(envName, newValue, scope);
        }
        else if (linux)
        {
            File.AppendAllText(bashrc, "export PATH=$PATH:~/.HISS/Language/ \n");
        }


        fs.Dispose();

        // installResources(path);

    }

    public static async void installResources(string path)
    {

        resourcesBar = new ProgressBar();
        WebClient client = new WebClient();

        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(resourcesProgressChanged);

        Task resourcesTask = client.DownloadFileTaskAsync(resourcesUri, path + @$"{ps}Resources.zip");

        resourcesTask.Wait();

        Console.WriteLine("HISS resources install complete");

        FileStream fs = new FileStream(path + @$"{ps}Resources.zip", FileMode.Open);
        ZipArchive archive = new ZipArchive(fs);
        extractToDirectory(archive, path + @$"{ps}Resources", true);
    }


    public static void uninstall(string path)
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

        if (windows)
        {
            string envName = "PATH";
            var scope = EnvironmentVariableTarget.Machine; // or User
            var oldValue = Environment.GetEnvironmentVariable(envName, scope);
            var newValue = oldValue.Replace(@$"{path}\Language\", "").Replace(@$"{path}\HIP\", "");
            Environment.SetEnvironmentVariable(envName, newValue, scope);
        }
        else if (linux)
        {
            string hipEnv = "export PATH=$PATH:~/.HISS/HIP/";
            string langEnv = "export PATH=$PATH:~/.HISS/Language/";
            string text = File.ReadAllText(bashrc);
            text = text.Replace(hipEnv, "");
            text = text.Replace(langEnv, "");
            File.WriteAllText(bashrc, text);
        }
    }


    public static string checkOs()
    {
        Console.WriteLine($"linux: {linux}");
        if (linux)
        {
            Console.WriteLine("linux detected with arch of " + RuntimeInformation.OSArchitecture.ToString());
            if (RuntimeInformation.OSArchitecture.ToString() == "X64")
            {
                return "linux-x64";
            }
        }
        else if (windows)
        {
            return RuntimeInformation.RuntimeIdentifier;
        }
        return "unknown";
    }

    public static void extractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
    {
        if (!overwrite)
        {
            archive.ExtractToDirectory(destinationDirectoryName);
            return;
        }

        DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
        string destinationDirectoryFullPath = di.FullName;

        foreach (ZipArchiveEntry file in archive.Entries)
        {
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
