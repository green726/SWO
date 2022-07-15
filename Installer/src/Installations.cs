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
        System.IO.Directory.CreateDirectory(path + @"\HIP\");
        Console.WriteLine("pulling file from: " + HIPUri);
        client.DownloadFile(HIPUri, path + @$"\HIP-{rid}.zip");
        //other HIP install stuff comes before the language install
        ZipFile.ExtractToDirectory(path + @$"\HIP-{rid}.zip", path + @"\HIP\");
        Environment.SetEnvironmentVariable("HIP", path + @"\HIP\");

        installLanguage(path);
    }

    public static void installLanguage(string path)
    {
        System.IO.Directory.CreateDirectory(path + @"\Language\");
        client.DownloadFile(languageUri, path + @$"\Language-{rid}.zip");
        ZipFile.ExtractToDirectory(path + $@"\Language-{rid}.zip", path + @"\Language\");
        Environment.SetEnvironmentVariable("HISS", path + @"\Language\");

    }
}
