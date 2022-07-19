using Spectre.Console;

public static class Prompt
{
    public static void getOptions()
    {

        Settings settings;
        bool uninstall = AnsiConsole.Confirm("Would you like to uninstall HISS?", false);

        if (uninstall)
        {
            settings = new Settings { uninstall = true };
            Util.figureOutSettings(settings);
            return;
        }

        string defaultPath = Util.evaluatePath();
        string path = AnsiConsole.Ask<string>("What [blue]path[/] would you like to install [green]HISS[/] to? (Leave for default path)", defaultPath);

        if (path == "")
        {
            path = defaultPath;
        }

        bool installHIP = AnsiConsole.Confirm("Install HIP (Highly Inefficient Packages) - The HISS package manager");
        bool installResources = AnsiConsole.Confirm("Install the HISS Resources (Templates, etc) - This is required for many HISS features but can be ignored for a greater degree of customization");

        settings = new Settings { uninstall = uninstall, installPath = path, installHIP = installHIP, installResources = installResources };
        Util.figureOutSettings(settings);
    }
}
