public class GeneralModel
{
    public TypoModel? typo { get; set; } = new TypoModel();
    public ImportModel? import { get; set; } = new ImportModel();
}

public class TypoModel
{
    public bool enabled { get; set; }
}

public class ImportModel
{
    public string keyword { get; set; } = "import";
}
