namespace ConfigModel;

public class VariableModel
{
    public VariableModelDeclaration declaration { get; set; } = new VariableModelDeclaration();
}

public class VariableModelDeclaration
{
    public List<string> order { get; set; } = new List<string>() { "keyword", "type", "name", "assignment", "value" };

    public bool reorder { get; set; } = false;

    public VariableDeclarationKeyword keyword { get; set; } = new VariableDeclarationKeyword();
}

public class VariableDeclarationKeyword
{
    public bool forced { get; set; } = true;
    public string constant { get; set; } = "const";
    public string mutable { get; set; } = "var";
    public bool mutableIsSymbol = false;
}
