namespace ConfigModel;

using System.Runtime.Serialization;

public class ConfigModel
{
    [DataMember(Name = "function")]
    public FunctionModel function { get; set; } = new FunctionModel();

    public LoopModel loop { get; set; } = new LoopModel();

    [DataMember(Name = "if")]
    public IfModel ifModel { get; set; } = new IfModel();

    public VariableModel variable { get; set; } = new VariableModel();

    public GeneralModel general { get; set; } = new GeneralModel();
}
