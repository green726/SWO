using Spectre.Console.Cli;

public class CLI
{
    CommandApp app;

    public CLI(string[] args)
    {
        app = new CommandApp();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.AddCommand<NewProjectCommand>("new");
            config.AddCommand<CompileCommand>("compile");
            config.AddCommand<RunCommand>("run");
            config.AddCommand<LookupErrorCommand>("error");
            config.AddCommand<TranspileCommand>("transpile");
        });
        app.Run(args);
    }
}




