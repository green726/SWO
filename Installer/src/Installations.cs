using System.Net;
using System.IO.Compression;
using Spectre.Console;
using System.Runtime.InteropServices;
using System.Diagnostics;

public static class Installations
{
    public static bool windows = false;
    public static bool linux = false;

    public static async Task clone(List<SWOComponent> comps, string branch)
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
                   foreach (SWOComponent comp in comps)
                   {
                       var task = ctx.AddTask(comp.downloadTaskName);
                       tasks.Add(comp.clone(task, branch));
                   }
                   await Task.WhenAll(tasks);
               });

    }

    public static async Task build(List<SWOComponent> comps, string os)
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
                   foreach (SWOComponent comp in comps)
                   {
                       var task = ctx.AddTask(comp.downloadTaskName);
                       tasks.Add(comp.build(task, os));
                   }
                   await Task.WhenAll(tasks);
               });
    }

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
            string bashrc = @$"{Environment.GetEnvironmentVariable("HOME")}/.bashrc";
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
    public string cloneTaskName;
    public string buildTaskName;
    public string cloneURL;
    public double currentDownloadPercent = 0;
    public double previousDownloadPercent = 0;
    public string sourceFolderName;
    public bool doBuild = true;

    public ProgressTask downloadProgressTask;

    public SWOComponent(string webPath, string installPath, string downloadPath, string installTaskName, string downloadTaskName, string cloneTaskName, string buildTaskName, string cloneURL = "", string sourceFolderName = "", bool build = true)
    {
        this.uri = new Uri(webPath);
        this.installPath = installPath;
        this.cloneURL = cloneURL;
        this.downloadPath = downloadPath;
        this.installTaskName = installTaskName;
        this.downloadTaskName = downloadTaskName;
        this.cloneTaskName = cloneTaskName;
        this.buildTaskName = buildTaskName;
        this.doBuild = build;
        this.sourceFolderName = sourceFolderName;
    }

    public Task clone(ProgressTask task, string branch)
    {
        Directory.CreateDirectory(Path.GetFullPath($"{installPath}/clone"));
        if (cloneURL == null || cloneURL == "")
        {
            task.StopTask();
            return Task.CompletedTask;
        }
        string gitCloneCommand = $"clone -b {branch} {cloneURL} {installPath}/clone";
        Process cloneProc = new Process();
        cloneProc.StartInfo.FileName = "git";
        cloneProc.StartInfo.Arguments = gitCloneCommand;
        cloneProc.Start();
        cloneProc.WaitForExit();

        task.StopTask();

        return Task.CompletedTask;
    }

    public Task build(ProgressTask task, string os)
    {
        task.MaxValue = 10;
        if (!this.doBuild)
        {
            task.StopTask();
            return Task.CompletedTask;
        }
        //build code goes here

        string projPath = $"{installPath}/clone/{sourceFolderName}";
        string[] projFiles = System.IO.Directory.GetFiles(projPath, "*.csproj");

        if (projFiles.Length > 1)
        {
            throw new ArgumentException("Multiple csproj files found in build target directory");
        }
        string projFileSingle = Environment.ExpandEnvironmentVariables(projFiles[0]);

        Process proc = new Process();
        proc.StartInfo.FileName = "dotnet";
        proc.StartInfo.Arguments = $"publish {projFileSingle} -r {os} --output {projPath}/publish";
        proc.StartInfo.RedirectStandardOutput = true;
        proc.Start();
        proc.WaitForExit();
        // Project proj = new Project(projFileSingle, new Dictionary<string, string>(), "6.0");
        // Console.WriteLine("building");
        // proj.SetGlobalProperty("Configuration", "Release");
        // bool built = proj.Build();

        task.Increment(8);
        //copy folder and delete code goes here

        Util.CopyFilesRecursively(new DirectoryInfo($"{projPath}/publish"), new DirectoryInfo(installPath));
        task.Increment(1);
        Directory.Delete($"{installPath}/clone", true);
        task.Increment(1);

        return Task.CompletedTask;
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
