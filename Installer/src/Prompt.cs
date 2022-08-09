using Spectre.Console;

public static class Prompt
{
    public static void getOptions()
    {

        Settings settings;
        bool uninstall = AnsiConsole.Confirm("Would you like to uninstall SWO?", false);

        if (uninstall)
        {
            settings = new Settings { uninstall = true };
            Util.figureOutSettings(settings);
            return;
        }

        string defaultPath = Util.evaluatePath();
        string path = AnsiConsole.Ask<string>("What [blue]path[/] would you like to install [green]SWO[/] to? (Leave blank for default path)", defaultPath);

        if (path == "")
        {
            path = defaultPath;
        }

        bool installSAP = AnsiConsole.Confirm("Install SAP (Highly Inefficient Packages) - The SWO package manager");
        bool installResources = AnsiConsole.Confirm("Install the SWO Resources (Templates, etc) - This is required for many SWO features but can be ignored for a greater degree of customization");

        settings = new Settings { uninstall = uninstall, installPath = path, dontInstallSAP = !installSAP, dontInstallResources = !installResources };
        Util.figureOutSettings(settings);
    }
}
