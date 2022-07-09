namespace ConfigModel;

public class IfModel
{
    public IfDeclaration? declaration { get; set; } = new IfDeclaration();

    public IfBody? body { get; set; } = new IfBody();
}

public class IfDeclaration
{
    public List<string>? delimeters { get; set; } = new List<string>() { "(", ")" };
}

public class IfBody
{
    public List<string>? delimeters { get; set; } = new List<string>() {"{", "}"};
}
