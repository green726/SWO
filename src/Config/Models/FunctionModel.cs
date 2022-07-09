using System.Runtime.Serialization;

// [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2200:Rethrow to preserve stack details", Justification = "Not production code.")]

public class FunctionModel
{
    [DataMember(Name = "declaration")]
    public FunctionDeclarationModel? declaration { get; set; }

    [DataMember(Name = "calling")]
    public FunctionCallingModel? calling { get; set; }
}

public class FunctionDeclarationModel
{
    public string? marker { get; set; }
    public FunctionDeclarationArgs? args { get; set; }
    public FunctionDeclarationBody? body { get; set; }
}

public class FunctionDeclarationArgs
{
    public List<string>? delimeters { get; set; }

    public string? seperator { get; set; }
}

public class FunctionDeclarationBody
{
    public List<string>? delimeters { get; set; }
}

public class FunctionCallingModel
{
    public FunctionCallingArgs? args { get; set; }

    public FunctionCallingBuiltin? builtin { get; set; }
}

public class FunctionCallingArgs
{
    [DataMember(Name = "delimeters")]
    public List<string>? delimeters { get; set; }

    public string? seperator { get; set; }
}

public class FunctionCallingBuiltin
{
    public FunctionCallingBuiltinMarker? marker { get; set; }
}

public class FunctionCallingBuiltinMarker
{
    public string? value { get; set; }
    public bool? enabled { get; set; }
    public string? location { get; set; }
}
