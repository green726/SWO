using System.Net;
using System.IO.Compression;

public static class Installations
{
    public static WebClient client = new WebClient();
    public static Uri HIPUri = new Uri("https://github.com/green726/HISS/HIP");
    public static Uri languageUri = new Uri("https://github.com/green726/HISS/Language");


    public static void installHIP(string path)
    {
        client.DownloadFileAsync(HIPUri, path + @"/HIP.zip");
        //other HIP install stuff comes before the language install
        ZipFile.ExtractToDirectory(path )


        installLanguage(path);
    }

    public static void installLanguage(string path)
    {
        client.DownloadFileAsync(languageUri, path + @"/Language.zip");

    }
}
