using System.Net;
using System.IO.Compression;

public static class Installations
{
    public static WebClient client = new WebClient();
    public static Uri HIPUri = new Uri("https://github.com/green726/HISS/releases/download/HIP.zip");
    public static Uri languageUri = new Uri("https://github.com/green726/HISS/releases/download/Language.zip");

    public static OperatingSystem os = Environment.OSVersion;


    public static void installHIP(string path)
    {
        client.DownloadFileAsync(HIPUri, path + @$"/HIP-{os}.zip");
        //other HIP install stuff comes before the language install
        ZipFile.ExtractToDirectory(path + @"/HIP.zip", path + @"/HIP/");
        Environment.SetEnvironmentVariable("HIP", path + @"/HIP/");

        installLanguage(path);
    }

    public static void installLanguage(string path)
    {
        client.DownloadFileAsync(languageUri, path + @$"/Language-{os}.zip");
        ZipFile.ExtractToDirectory(path + @"/Language.zip", path + @"/Language/");
        Environment.SetEnvironmentVariable("HISS", path + @"/Language/");

    }
}
