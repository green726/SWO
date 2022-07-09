using System.Runtime.Serialization;

public class LoopModel
{
    [DataMember(Name = "for")]
    public ForModel? forModel { get; set; }
}

public class ForModel
{
    public ForDeclaration? declaration { get; set; }
    public ForBody? body { get; set; }
}

public class ForDeclaration
{
    public List<string>? delimeters { get; set; }
}

public class ForBody
{
    public List<string>? delimeters { get; set; }
}
