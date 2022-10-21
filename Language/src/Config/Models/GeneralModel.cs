public class GeneralModel
{
    public TypoModel typo { get; set; } = new TypoModel();
    public ImportModel import { get; set; } = new ImportModel();
    public TopLevelModel topLevel { get; set; } = new TopLevelModel();

    public NullModel nulls { get; set; } = new NullModel();

    public CommentModel comment { get; set; } = new CommentModel();

    public LinkerModel linker { get; set; } = new LinkerModel();

    public SemiColonModel semiColon { get; set; } = new SemiColonModel();

    public ProjectModel project { get; set; } = new ProjectModel();
}

public class ProjectModel
{
    public STDLibModel STDLib { get; set; } = new STDLibModel();
}

public class STDLibModel
{
    public bool include { get; set; } = false;
}

public class SemiColonModel
{
    //Optional, Forced, None
    public string mode { get; set; } = "Optional";

}

public class NullModel
{
    public string keyword { get; set; } = "null";

    public bool enabled { get; set; } = true;
}

public class CommentModel
{
    public string singleLine { get; set; } = "//";
    public string multiLineOpen { get; set; } = "/*";
    public string multiLineClose { get; set; } = "*/";
}

public class LinkerModel
{
    public string type { get; set; } = "gcc";
    public bool auto { get; set; } = true;

    public string path { get; set; } = "";

    public string args { get; set; } = "";

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
                    args = "-O -no-pie -o";
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
