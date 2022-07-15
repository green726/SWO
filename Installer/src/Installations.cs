using System.Net;
using System.IO.Compression;
using System.Runtime.InteropServices;

public static class Installations
{
    public static WebClient client = new WebClient();

    public static string rid = RuntimeInformation.RuntimeIdentifier;


    public static Uri HIPUri = new Uri(@$"https://github.com/green726/HISS/releases/latest/download/HIP-{rid}.zip");
    public static Uri languageUri = new Uri(@$"https://github.com/green726/HISS/releases/latest/download/Language-{rid}.zip");



    public static void installHIP(string path)
    {
        // System.IO.Directory.CreateDirectory(path + @"\HIP\");
        Console.WriteLine("pulling file from: " + HIPUri);
        client.DownloadFile(HIPUri, path + @$"\HIP-{rid}.zip");


        FileStream fs = new FileStream(path + @$"\HIP-{rid}.zip", FileMode.Open);
        ZipArchive archive = new ZipArchive(fs);
        ExtractToDirectory(archive, path + @"\HIP", true);


        string envName = "PATH";
        var scope = EnvironmentVariableTarget.Machine; // or User
        var oldValue = Environment.GetEnvironmentVariable(envName, scope);
        var newValue = oldValue + @$"{path}\HIP\;";
        Environment.SetEnvironmentVariable(envName, newValue, scope);

        fs.Dispose();
        installLanguage(path);
    }

    public static void installLanguage(string path)
    {
        // System.IO.Directory.CreateDirectory(path + @"\Language\");
        client.DownloadFile(languageUri, path + @$"\Language-{rid}.zip");


        FileStream fs = new FileStream(path + $@"\Language-{rid}.zip", FileMode.Open);
        ZipArchive archive = new ZipArchive(fs);
        ExtractToDirectory(archive, path + @"\Language", true);

        string envName = "PATH";
        var scope = EnvironmentVariableTarget.Machine; // or User
        var oldValue = Environment.GetEnvironmentVariable(envName, scope);
        var newValue = oldValue + @$"{path}\Language\;";
        Environment.SetEnvironmentVariable(envName, newValue, scope);

        fs.Dispose();
    }

    public static void ExtractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
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
