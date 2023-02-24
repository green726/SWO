using System.Diagnostics;

public class CParser
{

    public static void parse(string path)
    {
        Process process = new Process();

        process.StartInfo.FileName = Config.settings.general.linker.path;
        // process.StartInfo.Arguments = $" {settings.resultFileName} {fullObjectPath}";

    }
}
