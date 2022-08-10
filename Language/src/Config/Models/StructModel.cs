public class StructModel
{
    public StructDeclarationModel declaration { get; set; } = new StructDeclarationModel();
}

public class StructDeclarationModel
{
    public string keyword { get; set; } = "struct";
}
