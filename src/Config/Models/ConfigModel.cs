using System.Runtime.Serialization;

public class ConfigModel
{
    [DataMember(Name = "functions")]
    public FunctionModel functions { get; set; }
}
