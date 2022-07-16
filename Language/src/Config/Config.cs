using Tomlyn;

public static class Config
{
    public static ConfigModel.ConfigModel settings;

    public static void initialize(string configFile)
    {
        string configInput = File.ReadAllText(configFile);

        settings = Toml.ToModel<ConfigModel.ConfigModel>(configInput);

        Console.WriteLine($"Function calling builtin marker enabled {settings.function.calling.builtin.marker.enabled}");
    }

}


