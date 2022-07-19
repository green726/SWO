using System.Net;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Spectre.Console;

public static class InstallationsNew
{
    public static void download(List<HISSComponent> downloadComponents)
    {

        foreach (HISSComponent component in downloadComponents)
        {
            component.download();
        }

        AnsiConsole.Progress().StartAsync(async ctx =>
        {
            List<ProgressTask> tasks = new List<ProgressTask>();
            int index = 0;

            foreach (HISSComponent component in downloadComponents)
            {
                tasks.Add(ctx.AddTask(component.downloadTaskName));
                index++;
            }
        });

    }



    public static void install(List<HISSComponent> installComponents)
    {

        foreach (HISSComponent component in installComponents)
        {
            component.install();
        }

        AnsiConsole.Progress().StartAsync((Func<ProgressContext, Task>)(async ctx =>
        {
            List<ProgressTask> tasks = new List<ProgressTask>();
            int index = 0;

            foreach (HISSComponent component in installComponents)
            {
                tasks.Add(ctx.AddTask(component.installTaskName));
                index++;
            }
        }));

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

    public HISSComponent(string webPath, string installPath, string downloadPath, string installTaskName, string downloadTaskName)
    {
        this.uri = new Uri(webPath);
        this.installPath = installPath;
        this.downloadPath = downloadPath;
        this.installTaskName = installTaskName;
        this.downloadTaskName = downloadTaskName;
    }

    public void install()
    {
        FileStream fs = new FileStream(downloadPath, FileMode.Open);
        ZipArchive archive = new ZipArchive(fs);
        Util.extractToDirectory(archive, installPath, true);
    }

    public void download()
    {
        WebClient client = new WebClient();
        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.downloadProgressChanged);
        client.DownloadFileAsync(this.uri, this.downloadPath);

    }

    public void downloadProgressChanged(object sender, DownloadProgressChangedEventArgs arg)
    {

    }
}
