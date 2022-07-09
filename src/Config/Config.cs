using Tomlyn;

public static class Config
{
    public static ConfigModel options;

    public static void initialize()
    {
        string configInput = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "../../../../src/Config/Test.toml");

        options = Toml.ToModel<ConfigModel>(configInput);

        Console.WriteLine($"Function calling builtin marker enabled {options.function.calling.builtin.marker.enabled}");
    }

}


