using System.Runtime.InteropServices;
using System.IO.Compression;

public static class Util
{
    public static void figureOutSettings(Settings settings)
    {
        Installations.resources = settings.installResources;
        if (settings.uninstall)
        {
            Installations.uninstall(settings.installPath);
            Environment.Exit(0);
        }

        System.IO.Directory.CreateDirectory(settings.installPath);

        if (settings.installHIP)
        {
            Installations.downloadHIP(settings.installPath);
        }
        else
        {
            Installations.downloadLanguage(settings.installPath);
        }

    }

    public static string evaluatePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.HISS");
        }
        else
        {
            return $@"{Environment.GetEnvironmentVariable("HOME")}/.HISS";
        }

    }


    public static async Task extractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
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
