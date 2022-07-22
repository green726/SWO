public class GeneralModel
{
    public TypoModel? typo { get; set; } = new TypoModel();
    public ImportModel? import { get; set; } = new ImportModel();
    public TopLevelModel topLevel { get; set; } = new TopLevelModel();
}

public class TopLevelModel
{
    public bool allow {get; set; }= false;
}

public class TypoModel
{
    public bool enabled { get; set; }
}

public class ImportModel
{
    public string keyword { get; set; } = "import";

    public ImportIgnoreModel ignore { get; set; } = new ImportIgnoreModel();
}

public class ImportIgnoreModel
{
    public string keyword { get; set; } = "private";
}
