using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;
using Newtonsoft.Json;

public class LookupErrorCommand : Command<LookupErrorCommandSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] LookupErrorCommandSettings settings)
    {
        string textToDisplay = getJSON(settings.errorCode);

        AnsiConsole.MarkupLine($"[green]Query results for error code {settings.errorCode}:[/]\n{textToDisplay}");

        return 0;
    }

    public static string getJSON(string code)
    {
        string storedJSONText = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "./src/CompilerResources/ErrorCodes.json");

        JsonTextReader reader = new JsonTextReader(new StringReader(storedJSONText));
        while (reader.Read())
        {
            if (reader.Value == null)
            {
                continue;
            }
            else
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    if (reader.Value!.ToString() == code)
                    {
                        reader.Read();
                        return (string)reader.Value!;
                    }
                }
            }
        }

        AnsiConsole.MarkupLine($"[red]Unknown error code({code}) queried[/]");
        Environment.Exit(0);
        return "";
    }
}
