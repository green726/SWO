namespace ConfigModel;

using System.Runtime.Serialization;

public class LoopModel
{
    [DataMember(Name = "for")]
    public ForModel forModel { get; set; } = new ForModel();
}

public class ForModel
{
    public ForDeclaration declaration { get; set; } = new ForDeclaration();
    public ForBody body { get; set; } = new ForBody();
}

public class ForDeclaration
{
    public List<string> delimeters { get; set; } = new List<string>() { "(", ")" };
}

public class ForBody
{
    public List<string> delimeters { get; set; } = new List<string>() {"{", "}"};
}
