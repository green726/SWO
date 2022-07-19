using System.Net;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Spectre.Console;

public static class Installations
{

    private static bool installingHIP = false;

    public static string os;

    private static int maxHIPFiles = 0;
    private static int currentHIPFile = 0;

    private static int maxLanguageFiles = 0;
    private static int currentLanguageFile = 0;

    private static int maxResourcesFiles = 0;
    private static int currentResourceFile = 0;

    public static Uri? HIPUri;
    public static Uri? languageUri;
    public static Uri? resourcesUri;

    public static bool windows;
    public static bool linux;

    public static string ps; //path seperator

    public static string bashrc = @$"{Environment.GetEnvironmentVariable("HOME")}/.bashrc";

    public static bool resources = true;

    private static int prevHIP = 0;
    private static int currentHIP = 0;
    public static void HIPProgressChanged(object sender, DownloadProgressChangedEventArgs arg)
    {
        prevHIP = currentHIP;
        currentHIP = arg.ProgressPercentage;
    }

    private static int prevLang = 0;
    private static int currentLang = 0;
    public static void languageProgressChanged(object sender, DownloadProgressChangedEventArgs arg)
    {
        prevLang = currentLang;
        currentLang = arg.ProgressPercentage;
    }

    private static int prevResources = 0;
    private static int currentResources = 0;
    public static void resourcesProgressChanged(object sender, DownloadProgressChangedEventArgs arg)
    {
        prevResources = currentResources;
        currentResources = arg.ProgressPercentage;
    }

    public static void init()
    {
        linux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        os = checkOs();


        resourcesUri = new Uri(@$"https://github.com/green726/HISS/releases/latest/download/Resources.zip");
        languageUri = new Uri(@$"https://github.com/green726/HISS/releases/latest/download/Language-{os}.zip");
        HIPUri = new Uri(@$"https://github.com/green726/HISS/releases/latest/download/HIP-{os}.zip");

        ps = windows ? @"\" : "/";
    }


    public static async void downloadHIP(string path)
    {
        installingHIP = true;
        WebClient client = new WebClient();

        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(HIPProgressChanged);

        Task HIPTask = client.DownloadFileTaskAsync(HIPUri, path + @$"{ps}HIP-{os}.zip");

        downloadLanguage(path);

        if (!resources)
        {
            // Asynchronous
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    // Define tasks
                    var task1 = ctx.AddTask("[green]Downloading HIP[/]");
                    task1.MaxValue = 100;

                    var task2 = ctx.AddTask("[blue]Downloading HISS Language[/]");
                    task2.MaxValue = 100;

                    // installLanguage(path);

                    while (!ctx.IsFinished)
                    {
                        task1.Increment(currentHIP - prevHIP);
                        task2.Increment(currentLang - prevLang);
                    }
                });

        }
        else
        {
            // Asynchronous
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    // Define tasks
                    var task1 = ctx.AddTask("[green]Downloading HIP[/]");
                    task1.MaxValue = 100;

                    var task2 = ctx.AddTask("[blue]Downloading HISS Language[/]");
                    task2.MaxValue = 100;

                    var task3 = ctx.AddTask("[purple]Downloading HISS Resources[/]");

                    // installLanguage(path);

                    while (!ctx.IsFinished)
                    {
                        task1.Increment(currentHIP - prevHIP);
                        task2.Increment(currentLang - prevLang);
                        task3.Increment(currentResources - prevResources);
                    }
                });
        }

        installHIP(path);

    }

    public static async void installHIP(string path)
    {
        FileStream fs = new FileStream(path + @$"{ps}HIP-{os}.zip", FileMode.Open);
        ZipArchive archive = new ZipArchive(fs);
        extractToDirectory(archive, path + @$"{ps}HIP", true);

        installLanguage(path);

        await AnsiConsole.Progress().StartAsync(async ctx =>
        {
            var task1 = ctx.AddTask("[green]Installing HIP[/]");
            task1.MaxValue = 100;

            var task2 = ctx.AddTask("[blue]Installing HISS Language[/]");
            task2.MaxValue = 100;

            var task3 = ctx.AddTask("[purple]Installing HISS Resources[/]");
            task3.MaxValue = 100;

            while (!ctx.IsFinished)
            {
                task1.Increment();
                task2.Increment();
                task3.Increment();
            }
        });


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

    }

    public static async void downloadLanguage(string path)
    {
        WebClient client = new WebClient();

        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(languageProgressChanged);
        Task langTask = client.DownloadFileTaskAsync(languageUri, path + @$"{ps}Language-{os}.zip");

        if (resources)
        {
            downloadResources(path);
        }

        if (!installingHIP && resources)
        {
            // Asynchronous
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var task1 = ctx.AddTask("[blue]Downloading HISS Language[/]");
                    task1.MaxValue = 100;

                    var task2 = ctx.AddTask("[purple]Downloading HISS Resources[/]");
                    task2.MaxValue = 100;


                    while (!ctx.IsFinished)
                    {
                        task1.Increment(currentLang - prevLang);
                        task2.Increment(currentResources - prevResources);
                    }
                });
            installLanguage(path);
            installResources(path);
        }
        else if (!installingHIP && !resources)
        {
            // Asynchronous
            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var task1 = ctx.AddTask("[blue]Downloading HISS Language[/]");
                    task1.MaxValue = 100;

                    while (!ctx.IsFinished)
                    {
                        task1.Increment(currentLang - prevLang);
                    }
                });
            installLanguage(path);
        }

    }

    public static async void installLanguage(string path)
    {
        FileStream fs = new FileStream(path + $@"{ps}Language-{os}.zip", FileMode.Open);
        ZipArchive archive = new ZipArchive(fs);
        extractToDirectory(archive, path + @$"{ps}Language", true);

        if (resources) {
        installResources(path);
        }

        if (windows)
        {
            string envName = "PATH";
            var scope = EnvironmentVariableTarget.Machine; // or User
            var oldValue = Environment.GetEnvironmentVariable(envName, scope);
            var newValue = oldValue + @$"{path}\Language\;";
            Environment.SetEnvironmentVariable(envName, newValue, scope);
        }
        else if (linux)
        {
            File.AppendAllText(bashrc, "export PATH=$PATH:~/.HISS/Language/ \n");
        }

        fs.Dispose();

    }

    public static async void downloadResources(string path)
    {

        WebClient client = new WebClient();

        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(resourcesProgressChanged);

        Task resourcesTask = client.DownloadFileTaskAsync(resourcesUri, path + @$"{ps}Resources.zip");

        // resourcesTask.Wait();
    }

    public static async void installResources(string path)
    {
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
        if (linux)
        {
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

    public static async Task extractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite, Step step)
    {
        object fileNum;
        switch (step)
        {
            case Step.HIP:
                maxHIPFiles = archive.Entries.Count();
                fileNum = currentHIPFile;
                break;
            case Step.Language:
                maxLanguageFiles = archive.Entries.Count();
                fileNum = currentLanguageFile;
                break;
            case Step.Resources:
                maxResourcesFiles = archive.Entries.Count();
                fileNum = currentResourceFile;
                break;
        }
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

public enum Step
{
    HIP,
    Language,
    Resources
}
