public class GeneralModel
{
    public TypoModel typo { get; set; } = new TypoModel();
    public ImportModel import { get; set; } = new ImportModel();
    public TopLevelModel topLevel { get; set; } = new TopLevelModel();

    public LinkerModel linker { get; set; } = new LinkerModel();
}

public class LinkerModel
{
    public string type { get; set; } = "gcc";
    public bool auto { get; set; } = true;


    public string path { get; set; } = "";

    public LinkerModel()
    {
        if (path == "")
        {
            switch (this.type)
            {
                case "gcc":
                    switch (Util.checkOs())
                    {
                        case "win10-x64":
                            path = @"C:\ProgramData\chocolatey\lib\mingw\tools\install\mingw64\bin\gcc.exe";
                            break;
                        case "linux-x64":
                            path = @"/usr/bin/gcc";
                            break;
                    }
                    break;
            }
        }
    }
}

public class TopLevelModel
{
    public bool allow { get; set; } = false;
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
