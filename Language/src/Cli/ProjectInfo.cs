using System.Runtime.Serialization;
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

    public List<Library> libraries { get; set; } = new List<Library>();
    public string projectName { get; set; } = "unknown";

    public string configFilePath { get; set; } = "";

    public SWOFile entryFile { get; set; }

    public string path { get; set; } = "";

    public void write()
    {
        var tomlString = Toml.FromModel(this);
        // DebugConsole.Write("Toml string: \n" + tomlString);
        File.WriteAllText(@$"{path}/{projectName}.sproj", tomlString);
    }

    public ProjectInfo()
    {
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
}

public class SWOFile
{
    public string nameWithoutExtension { get; set; }
    public string name { get; set; }
    public string path { get; set; }

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
