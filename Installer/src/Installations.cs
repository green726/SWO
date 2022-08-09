using System.Net;
using System.IO.Compression;
using Spectre.Console;
using System.Runtime.InteropServices;

public static class Installations
{
    public static bool windows = false;
    public static bool linux = false;

    public static async Task download(List<SWOComponent> downloadComponents)
    {
        await AnsiConsole.Progress()
        .Columns(new ProgressColumn[]
        {
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new RemainingTimeColumn(),
            new SpinnerColumn(),
        }).
        StartAsync(async ctx =>
        {
            List<Task> tasks = new List<Task>();
            foreach (SWOComponent comp in downloadComponents)
            {
                var task = ctx.AddTask(comp.downloadTaskName);
                tasks.Add(comp.download(task));
            }
            await Task.WhenAll(tasks);
        });
    }


    public static async Task install(List<SWOComponent> installComponents)
    {

        await AnsiConsole.Progress().StartAsync(async ctx =>
        {
            List<Task> tasks = new List<Task>();
            foreach (SWOComponent comp in installComponents)
            {
                var task = ctx.AddTask(comp.installTaskName);
                tasks.Add(comp.install(task));
            }

            await Task.WhenAll(tasks);
        });


    }

    public static void addToPath(Settings settings)
    {
        string bashrc = @$"{Environment.GetEnvironmentVariable("HOME")}/.bashrc";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string envName = "PATH";
            var scope = EnvironmentVariableTarget.Machine; // or User
            var oldValue = Environment.GetEnvironmentVariable(envName, scope);
            var newValue = oldValue + @$"{settings.installPath}\Language\;";
            if (!settings.dontInstallSAP)
            {
                Console.WriteLine("adding SAP to path");
                newValue += @$"{settings.installPath}\SAP\;";
            }
            Environment.SetEnvironmentVariable(envName, newValue, scope);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (!settings.dontInstallSAP)
            {
                File.AppendAllText(bashrc, "export PATH=$PATH:~/.SWO/SAP/ \n");
            }
            File.AppendAllText(bashrc, "export PATH=$PATH:~/.SWO/Language/ \n");

        }

    }
}

public class SWOComponent
{
    public Uri uri;
    public string installPath;
    public string downloadPath;
    public string installTaskName;
    public string downloadTaskName;
    public double currentDownloadPercent = 0;
    public double previousDownloadPercent = 0;

    public ProgressTask downloadProgressTask;

    public SWOComponent(string webPath, string installPath, string downloadPath, string installTaskName, string downloadTaskName)
    {
        this.uri = new Uri(webPath);
        this.installPath = installPath;
        this.downloadPath = downloadPath;
        this.installTaskName = installTaskName;
        this.downloadTaskName = downloadTaskName;
    }

    public Task install(ProgressTask task)
    {
        FileStream fs = new FileStream(downloadPath, FileMode.Open);
        ZipArchive archive = new ZipArchive(fs);
        return Util.extractToDirectory(archive, installPath, true, ref task);
        // fs.DisposeAsync();
    }

    public Task download(ProgressTask task)
    {
        this.downloadProgressTask = task;
        WebClient client = new WebClient();
        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgressChanged);
        return client.DownloadFileTaskAsync(this.uri, this.downloadPath);
    }

    public void downloadProgressChanged(object sender, DownloadProgressChangedEventArgs arg)
    {
        this.previousDownloadPercent = this.currentDownloadPercent;
        this.currentDownloadPercent = arg.ProgressPercentage;

        this.downloadProgressTask.Increment(this.currentDownloadPercent - this.previousDownloadPercent);
    }
}


