namespace ConfigModel;

using System.Runtime.Serialization;

public class ConfigModel
{
    [DataMember(Name = "function")]
    public FunctionModel? function { get; set; }

    public LoopModel? loop { get; set; }

    [DataMember(Name = "if")]
    public IfModel? ifModel { get; set; }

    public VariableModel? variable { get; set; }

    public GeneralModel? general { get; set; }
}
