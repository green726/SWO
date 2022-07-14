using System.Net;

public static class Installations
{
    public static WebClient client = new WebClient();
    public static Uri HIPPath = new Uri("https://github.com/green726/HISS");
    public static Uri languagePath = new Uri("https://github.com/green726/HISS");


    public static void installHIP(string path)
    {
        client.DownloadFileAsync();
        installLanguage(path);
    }

    public static void installLanguage(string path)
    {
        client.DownloadFileAsync();
    }
}
