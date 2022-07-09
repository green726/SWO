using Tomlyn;
using Tomlyn.Model;

public static class Config
{

    public static void initialize()
    {
        string configInput = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "../../../../src/Config/Example.toml");

        var model = Toml.ToModel(configInput);

        Console.WriteLine(((TomlTable)model["functions"]));
    }
}
