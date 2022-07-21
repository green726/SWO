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
            Console.WriteLine("calling check path from template setter");
            checkPath();
            _template = value;
        }
    }

    public List<HISSFile> files { get; set; } = new List<HISSFile>();

    public List<Library> libraries { get; set; } = new List<Library>();
    public string projectName { get; set; } = "unknown";

    public string configFilePath { get; set; } = "";

    public HISSFile entryFile { get; set; }

    public string path { get; set; } = "";

    public void write()
    {
        var tomlString = Toml.FromModel(this);
        // Console.WriteLine("Toml string: \n" + tomlString);
        Console.WriteLine("writing toml string to " + @$"{path}{projectName}.hproj");
        File.WriteAllText(@$"{path}/{projectName}.hproj", tomlString);
    }

    public ProjectInfo()
    {
    }

    public void setConfig()
    {
        DirectoryInfo proj = new DirectoryInfo(path);
        if (proj.GetFiles().Contains(new FileInfo("config.toml")))
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
        files.Add(new HISSFile(name, path));
    }

    //NOTE: call this in the program main when an existing project info is read to check for any new files that were created
    public void checkPath()
    {
        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        string[] folderNames = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);
        Console.WriteLine("path filenames: " + files.ToString());

        foreach (string file in files)
        {
            HISSFile HISSfile = new HISSFile(Path.GetFileName(file), file);
            if (!this.files.Contains(HISSfile) && HISSfile.name.EndsWith("hiss"))
            {
                Console.WriteLine($"fileName: {file}");
                this.files.Add(HISSfile);
            }
        }
        Console.WriteLine("file count:" + this.files.Count());
    }

    public void findEntryFileFromTemplate()
    {
        string templateEntryName = this.template.entryFile.name;

        string relativeTemplateEntryPath = template.entryFile.path.Replace(template.path, "");
        string entryPath = this.path + relativeTemplateEntryPath;

        Console.WriteLine(entryPath);

        this.entryFile = files.Where(file => file.path == entryPath).ToArray()[0];
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

public class HISSFile
{
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

    public HISSFile(string name, string path)
    {
        this.name = name;
        this.path = path;
    }

    public HISSFile()
    {

    }
}
