public class Template
{
    public string type { get; set; }
    public string path { get; set; }
    public HISSFile entryFile { get; set; }


    public Template(string type)
    {
        this.type = type;
        //TODO: once HISS has actual releases - un comment this
        // path = @$"../Resources/Templates{type.ToString()}";
        path = OperatingSystem.IsWindows() ? Environment.ExpandEnvironmentVariables(@$"%USERPROFILE%\.HISS\Resources\Templates\{type}") : $"~/.HISS/Resources/Templates/{type}";
        this.entryFile = new HISSFile("Main", path + "/Main.hiss");
    }
}
