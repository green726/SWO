public class Template
{
    public string type { get; set; }
    public string path { get; set; }
    public SWOFile entryFile { get; set; }


    public Template(string type)
    {
        this.type = type;
        //TODO: once SWO has actual releases - un comment this
        // path = @$"../Resources/Templates{type.ToString()}";
        path = OperatingSystem.IsWindows() ? Environment.ExpandEnvironmentVariables(@$"%USERPROFILE%/.SWO/Resources/Templates/{type}") : Environment.ExpandEnvironmentVariables($"%HOME%/.SWO/Resources/Templates/{type}");
        this.entryFile = new SWOFile("Main", path + "/Main.hiss");
    }
}
