using System.Runtime.Serialization;
using Tomlyn;

public class ProjectInfo
{
    public Template template { get; set; } = new Template("blank");

    public HISSFile[] filesArr
    {
        get
        {
            return files.ToArray();
        }
        set { }
    }

    public Library[] librariesArr
    {
        get
        {
            return libraries.ToArray();
        }
        set { }
    }

    [IgnoreDataMember]
    public List<HISSFile> files { get; set; } = new List<HISSFile>();
    [IgnoreDataMember]
    public List<Library> libraries { get; set; } = new List<Library>();
    public string name { get; set; } = "unknown";

    public string configFilePath { get; set; } = "";

    public HISSFile entryFile;

    private string _path = "empty";
    public string path
    {
        get
        {
            return _path;
        }
        set
        {
            _path = value;
            Console.WriteLine("path setter called with val of: " + path);
            string[] fileNames = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            string[] folderNames = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);
            Console.WriteLine("path filenames: " + fileNames.ToString());

            foreach (string fileName in fileNames)
            {
                Console.WriteLine($"fileName: {fileName}");
                HISSFile file = new HISSFile(fileName, fileName);
                files.Add(file);
            }
        }

    }

    public void write()
    {
        var tomlString = Toml.FromModel(this);
        Console.WriteLine("Toml string: \n" + tomlString);
        File.WriteAllText(@$"{path}\{name}.hproj", tomlString);
    }

    public ProjectInfo()
    {
        configFilePath = Environment.CurrentDirectory + "/config.toml";
    }

    public void addLibrary(Library library)
    {
        libraries.Add(library);
    }

    public void addFile(string name, string path)
    {
        files.Add(new HISSFile(name, path));
    }

    public void recheckPath()
    {
        string[] fileNames = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        string[] folderNames = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);
        Console.WriteLine("path filenames: " + fileNames.ToString());

        foreach (string fileName in fileNames)
        {
            Console.WriteLine($"fileName: {fileName}");
            HISSFile file = new HISSFile(fileName, fileName);
            files.Add(file);
        }
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

}

public class HISSFile
{
    public string name { get; set; }
    public string path { get; set; }

    public HISSFile(string name, string path)
    {
        this.name = name;
        this.path = path;
    }
}
