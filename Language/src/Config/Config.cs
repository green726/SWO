using Tomlyn;

public static class Config
{
    public static ConfigModel.ConfigModel settings;
    public static ConfigModel.ConfigModel target;

    public static void initializeTranspiler(string configFile, string transpilerConfigFile = "")
    {
        if (transpilerConfigFile == "") {
            initializeStdForm(configFile);
            return;
        }
        string configInput = File.ReadAllText(configFile);

        settings = Toml.ToModel<ConfigModel.ConfigModel>(configInput);


        string targetInput = File.ReadAllText(transpilerConfigFile);

        target = Toml.ToModel<ConfigModel.ConfigModel>(targetInput);
    }

    private static void initializeStdForm(string configFile)
    {

        string configInput = File.ReadAllText(configFile);

        settings = Toml.ToModel<ConfigModel.ConfigModel>(configInput);

        target = new ConfigModel.ConfigModel();
    }

    public static void initialize(string configFile)
    {
        string configInput = File.ReadAllText(configFile);

        settings = Toml.ToModel<ConfigModel.ConfigModel>(configInput);
        settings.general.linker.checkPath();
    }

}


