public class IfModel
{
    public IfDeclaration? declaration { get; set; }

    public IfBody? body { get; set; }
}

public class IfDeclaration
{
    public List<string>? delimeters {get; set;}
}

public class IfBody
{
    public List<string>? delimeters {get; set; }
}
