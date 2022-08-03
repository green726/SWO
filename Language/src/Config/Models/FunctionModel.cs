namespace ConfigModel;

using System.Runtime.Serialization;

// [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2200:Rethrow to preserve stack details", Justification = "Not production code.")]

public class FunctionModel
{
    [DataMember(Name = "declaration")]
    public FunctionDeclarationModel declaration { get; set; } = new FunctionDeclarationModel();

    [DataMember(Name = "calling")]
    public FunctionCallingModel calling { get; set; } = new FunctionCallingModel();

    public FunctionReturnModel ret { get; set; } = new FunctionReturnModel();
}

public class FunctionReturnModel
{
    public string keyword { get; set; } = "return";

    public ReturnKeywordLocation location { get; set; } = ReturnKeywordLocation.Start;

    public FunctionReturnDynamicModel dynamic { get; set; } = new FunctionReturnDynamicModel();
}

public enum ReturnKeywordLocation
{
    Start,
    End
}

public class FunctionReturnDynamicModel
{
    public bool enable { get; set; } = false;

    public string keyword { get; set; } = "dynamic";

    public bool keywordRequired { get; set; } = true;
}


public class FunctionDeclarationModel
{
    public FunctionDeclarationMarker marker { get; set; } = new FunctionDeclarationMarker();
    public FunctionDeclarationArgs args { get; set; } = new FunctionDeclarationArgs();
    public FunctionDeclarationBody body { get; set; } = new FunctionDeclarationBody();
    public bool reorder { get; set; } = false;
    public ReturnTypeLocation returnTypeLocation { get; set; } = ReturnTypeLocation.Start;

    public string externKeyword {get; set;} = "extern";
}

public enum ReturnTypeLocation
{
    Start,
    End
}

public class FunctionDeclarationMarker
{
    public string value { get; set; } = "@";
    public bool word { get; set; } = false;
}

public class FunctionDeclarationArgs
{
    public List<string> delimeters { get; set; } = new List<string> { "(", ")" };

    public string seperator { get; set; } = ",";
}

public class FunctionDeclarationBody
{
    public List<string> delimeters { get; set; } = new List<string>() { "{", "}" };
}

public class FunctionCallingModel
{
    public FunctionCallingArgs args { get; set; } = new FunctionCallingArgs();

    public FunctionCallingBuiltin builtin { get; set; } = new FunctionCallingBuiltin();
}

public class FunctionCallingArgs
{
    [DataMember(Name = "delimeters")]
    public List<string> delimeters { get; set; } = new List<string>() { "(", ")" };

    public string seperator { get; set; } = ",";
}

public class FunctionCallingBuiltin
{
    public FunctionCallingBuiltinMarker marker { get; set; } = new FunctionCallingBuiltinMarker();
}

public class FunctionCallingBuiltinMarker
{
    public string value { get; set; } = "!";
    public bool enabled { get; set; } = true;
    public string location { get; set; } = "end";
}
