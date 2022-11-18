using System.Runtime.Serialization;
using Newtonsoft.Json;
using Tomlyn;

public class ProjectInfo
{
    [IgnoreDataMember]
    private Template _template = new Template("blank");
    [IgnoreDataMember]
    public Template template
    {
        get
        {
            return _template;
        }
        set
        {
            Util.copyDirectory(value.path, this.path, true, true);
            checkPath();
            _template = value;
        }
    }

    public List<SWOFile> files { get; set; } = new List<SWOFile>();

    public List<ASTFile> ASTFiles { get; set; } = new List<ASTFile>();

    public List<Library> libraries { get; set; } = new List<Library>();
    public string projectName { get; set; } = "unknown";

    public string configFilePath { get; set; } = "";

    public SWOFile entryFile { get; set; }

    public string path { get; set; } = "";

    public bool STDLibIncluded { get; set; } = true;

    public void write()
    {
        // var tomlString = Toml.FromModel(this);
        var jsonString = JsonConvert.SerializeObject(this);
        // DebugConsole.Write("Toml string: \n" + tomlString);
        File.WriteAllText(@$"{path}/{projectName}.sproj", jsonString);
    }

    public ProjectInfo()
    {
        entryFile = new SWOFile();
        // if (Config.settings.general.project.STDLib.include)
        // {
        //     if (!STDLibIncluded)
        //     {
        //         libraries.Add(new Library("stdlib", false));
        //     }
        // }
        // else
        // {
        //     if (STDLibIncluded)
        //     {
        //         foreach (Library lib in libraries)
        //         {
        //             if (lib.name == "stdlib")
        //             {
        //                 libraries.Remove(lib);
        //             }
        //         }
        //     }
        // }
    }

    public void setConfig()
    {
        if (File.Exists(Path.GetFullPath(@$"{path}/config.toml")))
        {
            configFilePath = path + "/config.toml";
        }
        else
        {
            configFilePath = Util.installPath + "/Resources/Config/config.toml";
        }
    }

    public void addLibrary(Library library)
    {
        libraries.Add(library);
    }

    public void addFile(string name, string path)
    {
        files.Add(new SWOFile(name, path));
    }

    //NOTE: call this in the program main when an existing project info is read to check for any new files that were created
    public void checkPath()
    {
        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        string[] folderNames = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            SWOFile SWOfile = new SWOFile(Path.GetFileName(file), file);
            if (!this.files.Contains(SWOfile) && SWOfile.name.EndsWith("swo"))
            {
                this.files.Add(SWOfile);
            }
        }
    }

    public void findEntryFileFromTemplate()
    {
        string templateEntryName = this.template.entryFile.name;

        string relativeTemplateEntryPath = template.entryFile.path.Replace(template.path, "");
        string entryPath = this.path + relativeTemplateEntryPath;

        this.entryFile = files.Where(file => Path.GetFullPath(file.path) == Path.GetFullPath(entryPath)).ToArray()[0];
    }

    public void addFilesFromDir(string dirPath)
    {
        List<string> filePaths = Directory.GetFiles(dirPath).ToList();
        foreach (string filePath in filePaths)
        {
            this.addFile(Path.GetFileName(filePath), filePath);
        }
    }
}

public class Library
{
    public string name { get; set; } = "";
    public bool foreign { get; set; } = false;
    public ASTFile libraryAST { get; set; }

    public Library(string name, bool foreign = false)
    {
        //TODO: search globally in the swo libraries for this name - if not found search locally in project
        // this.libraryAST = new ASTFile();
    }
}

public class ASTFile
{
    public string nameWithoutExtension { get; set; } = "";
    public string name { get; set; } = "";
    public string path { get; set; } = "";

    public Dictionary<string, AST.Prototype> prototypes { get; set; } = new Dictionary<string, AST.Prototype>();

    public Dictionary<string, AST.Struct> structs { get; set; } = new Dictionary<string, AST.Struct>();

    public static List<ASTFile> deserializeAll(string path)
    {
        string[] files = Directory.GetFiles(path, "*.ast.json", SearchOption.AllDirectories);
        List<ASTFile> returnList = new List<ASTFile>();

        foreach (string file in files)
        {
            string fileText = File.ReadAllText(file);

            returnList.Add(JsonConvert.DeserializeObject<ASTFile>(fileText)!);
        }
        return returnList;
    }

    public ASTFile()
    {
    }

    public ASTFile(Parser parser)
    {
        this.prototypes = parser.declaredFuncs;
        this.structs = parser.declaredStructs;
        DebugConsole.Write(this.prototypes.First().Value);

        this.nameWithoutExtension = parser.fileName;
        this.name = $"{nameWithoutExtension}.ast.json";
        this.path = parser.filePath;
        DebugConsole.Write(parser.filePath);
    }

    public void write()
    {
        var jsonString = JsonConvert.SerializeObject(this);
        DebugConsole.Write("path:" + path + " path end");
        string fileToWrite = path.Remove(path.Length - 4);
        File.WriteAllText(@$"{fileToWrite}.ast.json", jsonString);
    }
}

public class SWOFile
{
    public string nameWithoutExtension { get; set; } = "";
    public string name { get; set; } = "";
    public string path { get; set; } = "";

    [IgnoreDataMember]
    public string value
    {
        get
        {
            return File.ReadAllText(path);
        }
    }

    public SWOFile(string name, string path)
    {
        this.name = name;
        this.path = path;
        this.nameWithoutExtension = Path.GetFileNameWithoutExtension(path);
    }

    public SWOFile()
    {
    }
}
