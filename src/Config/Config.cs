using Tomlyn;
using Tomlyn.Model;

public static class Config
{

    public static void initialize()
    {
        string configInput = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "../../../../src/Config/Test.toml");

        ConfigModel model = Toml.ToModel<ConfigModel>(configInput);

        Console.WriteLine($"Function calling builtin marker enabled {model.functions.calling.builtin.marker.enabled}");


        throw new Exception();
    }

}


