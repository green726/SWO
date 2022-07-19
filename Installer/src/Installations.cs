using System.Net;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Spectre.Console;

public static class Installations
{
    public static Task download(List<HISSComponent> downloadComponents)
    {

        foreach (HISSComponent component in downloadComponents)
        {
            component.download();
        }

        AnsiConsole.Progress().StartAsync(async ctx =>
        {
            List<ProgressTask> tasks = new List<ProgressTask>();

            foreach (HISSComponent component in downloadComponents)
            {
                tasks.Add(ctx.AddTask(component.downloadTaskName));
            }

            while (!ctx.IsFinished)
            {
                for (int i = 0; i < downloadComponents.Count(); i++)
                {
                    tasks[i].Increment(downloadComponents[i].currentDownloadPercent - downloadComponents[i].previousDownloadPercent);
                }
            }
        });

        return Task.CompletedTask;

    }



    public static Task install(List<HISSComponent> installComponents)
    {

        foreach (HISSComponent component in installComponents)
        {
            component.install();
        }

        AnsiConsole.Progress().Start((Func<ProgressContext, Task>)(async ctx =>
        {
            List<ProgressTask> tasks = new List<ProgressTask>();

            foreach (HISSComponent component in installComponents)
            {
                tasks.Add(ctx.AddTask(component.installTaskName, maxValue: component.maxInstallFiles));
            }

            while (!ctx.IsFinished)
            {
                for (int i = 0; i < installComponents.Count(); i++)
                {
                    tasks[i].Value = installComponents[i].currentInstallFile;
                }
            }
        }));


        return Task.CompletedTask;

    }
}

public class HISSComponent
{
    public Uri uri;
    public string installPath;
    public string downloadPath;
    public string installTaskName;
    public string downloadTaskName;
    public double installTaskMaxValue = 0;
    public double currentDownloadPercent = 0;
    public double previousDownloadPercent = 0;

    public int currentInstallFile = 0;
    public int maxInstallFiles = 0;

    public Task downloadTask;

    public HISSComponent(string webPath, string installPath, string downloadPath, string installTaskName, string downloadTaskName)
    {
        this.uri = new Uri(webPath);
        this.installPath = installPath;
        this.downloadPath = downloadPath;
        this.installTaskName = installTaskName;
        this.downloadTaskName = downloadTaskName;
    }

    public async void install()
    {
        FileStream fs = new FileStream(downloadPath, FileMode.Open);
        ZipArchive archive = new ZipArchive(fs);
        Console.WriteLine($"extracting file with install task of name {installTaskName}");
        this.maxInstallFiles = archive.Entries.Count();
        // Util.extractToDirectory(archive, installPath, true, ref currentInstallFile);
        fs.DisposeAsync();
    }

    public void download()
    {
        WebClient client = new WebClient();
        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgressChanged);
        this.downloadTask = client.DownloadFileTaskAsync(this.uri, this.downloadPath);

    }

    public void downloadProgressChanged(object sender, DownloadProgressChangedEventArgs arg)
    {
        this.previousDownloadPercent = this.currentDownloadPercent;
        this.currentDownloadPercent = arg.ProgressPercentage;
    }
}


