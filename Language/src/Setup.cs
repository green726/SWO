using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Reflection;
using Tomlyn;

public static class Setup
{
    public static List<string> templates = new List<string>() { "" }; public static string runPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public static CompilerOptions parseOptions(string[] inputs)
    {
        CompilerOptions compilerOptions = new CompilerOptions();


        if (inputs.Length > 0 && inputs[0] == "new")
        {
            List<string> inputList = new List<string>(inputs);
            inputList.RemoveAt(0);
            createNewProject(inputList);
            Console.WriteLine("Successfully created a new HISS project");
            Environment.Exit(0);
        }

        System.Reflection.PropertyInfo currentProp = null;
        int index = 0;
        bool inputIsBool = false;
        bool inputBool = false;

        foreach (string input in inputs)
        {

            if (input == "true")
            {
                inputIsBool = true;
                inputBool = true;
            }
            else if (input == "false")
            {
                inputIsBool = true;
                inputBool = false;
            }
            else
            {
                inputIsBool = false;
            }
            if (currentProp != null)
            {
                if (inputIsBool)
                {
                    currentProp.SetValue(compilerOptions, inputBool);
                }
                else
                {
                    currentProp.SetValue(compilerOptions, input);
                }
                currentProp = null;
            }
            else if (input.StartsWith("--"))
            {
                string option = input.Substring(2);

                currentProp = compilerOptions.GetType().GetProperty(option);


                if (currentProp.PropertyType == typeof(bool))
                {
                    currentProp.SetValue(compilerOptions, true);
                    currentProp = null;
                }

                Console.WriteLine("current option changed to " + currentProp);
            }
            else
            {
                if (index > 1)
                {
                    throw new Exception("Unknown or illegal installer argument");
                }
                Console.WriteLine("set installer options options with index of " + index + " to input of " + input);
                if (inputIsBool)
                {
                    compilerOptions.GetType().GetProperty(compilerOptions.options[index]).SetValue(compilerOptions, inputBool);
                }
                else
                {
                    compilerOptions.GetType().GetProperty(compilerOptions.options[index]).SetValue(compilerOptions, input);
                }
            }
            index++;
        }

        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(compilerOptions))
        {
            string name = descriptor.Name;
            object value = descriptor.GetValue(compilerOptions);
            Console.WriteLine($"compiler option: {name} with a value of {value}");
        }

        throw new Exception();
        return compilerOptions;
    }

    public static void createNewProject(List<string> inputs)
    {
        ProjectInfo projectInfo = new ProjectInfo();
        Template template = null;
        int index = 0;

        foreach (string input in inputs)
        {
            switch (index)
            {
                case 0:
                    if (templates.Contains(input))
                    {
                        template = new Template(input);
                        projectInfo.template = template;
                    }
                    break;
                case 1:
                    if (Directory.Exists(input))
                    {
                        Console.WriteLine($"setting proj info path to {input}");
                        projectInfo.path = input;
                        Console.WriteLine($"set path sucessfully");
                    }
                    else
                    {

                    }
                    break;
                case 2:
                    projectInfo.name = input;
                    break;
            }
            index++;
        }
        if (projectInfo.path == "empty")
        {
            projectInfo.path = runPath;
        }


        Util.copyDirectory(projectInfo.template.path, projectInfo.path, true, true);
        addFilesFromDir(projectInfo, projectInfo.template.path);

        projectInfo.write();

    }

    public static void addFilesFromDir(ProjectInfo projectInfo, string dirPath)
    {
        List<string> filePaths = Directory.GetFiles(dirPath).ToList();
        foreach (string filePath in filePaths)
        {
            projectInfo.addFile(Path.GetFileName(filePath), filePath);
        }
    }

    public static ProjectInfo getProjectInfo()
    {
        return null;
    }
}



public class ProjectInfo
{
    public Template template { get; set; } = new Template(TemplateType.Blank);
    public List<InputFile> files { get; set; } = new List<InputFile>();
    public List<Library> libraries { get; set; } = new List<Library>();
    public string name { get; set; } = "unknown";

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
                InputFile file = new InputFile(fileName, fileName);
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

    }


    public void addLibrary(Library library)
    {
        libraries.Add(library);
    }

    public void addFile(string name, string path)
    {
        files.Add(new InputFile(name, path));
    }

    public void recheckPath()
    {
        string[] fileNames = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        string[] folderNames = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);
        Console.WriteLine("path filenames: " + fileNames.ToString());

        foreach (string fileName in fileNames)
        {
            Console.WriteLine($"fileName: {fileName}");
            InputFile file = new InputFile(fileName, fileName);
            files.Add(file);
        }
    }
}

public class Template
{
    public TemplateType type { get; set; }
    public string strType { get; set; }
    public string path { get; set; }


    public Template(TemplateType type)
    {
        this.type = type;
        this.strType = type.ToString();
        //TODO: once HISS has actual releases - un comment this
        // path = @$"../Resources/Templates{type.ToString()}";
        path = OperatingSystem.IsWindows() ? Environment.ExpandEnvironmentVariables(@$"%USERPROFILE%\.HISS\Resources\Templates\{type.ToString()}") : Environment.ExpandEnvironmentVariables(@$"%HOME%\.HISS\Resources\Templates\{type.ToString()}");
    }

    public Template(string type)
    {
        this.strType = type;
        switch (type)
        {
            case "blank":
                this.type = TemplateType.Blank;
                break;
            default:
                throw new Exception("Unknown template");
        }
        //TODO: once HISS has actual releases - un comment this
        // path = @$"../Resources/Templates{type.ToString()}";
        path = OperatingSystem.IsWindows() ? Environment.ExpandEnvironmentVariables(@$"%USERPROFILE%\.HISS\Resources\Templates\{type}") : Environment.ExpandEnvironmentVariables(@$"%HOME%\.HISS\Resources\Templates\{type}");
    }

}

public enum TemplateType
{
    Blank
}



public class Library
{

}

public class InputFile
{
    public string name { get; set; }
    public string path { get; set; }

    public InputFile(string name, string path)
    {
        this.name = name;
        this.path = path;
    }
}

public enum FileType
{
    Assembly,
    Object,
    Binary,
    LLVMIR,
    NativeExecutable,
}

public class CompilerOptions
{
    public string file { get; set; } = "";
    public string folder { get; set; } = "";
    public string configFile { get; set; } = "";

    public FileType targetFileType { get; set; } = FileType.NativeExecutable;
    public string targetFileName { get; set; } = "my-hiss-program";
    public string targetOSName { get; set; } = "";

    public string[] options = new string[] { "file", "targetFileName" };

    public CompilerOptions()
    {
        this.targetOSName = RuntimeInformation.RuntimeIdentifier;
        this.configFile = "../Resources/Config/config.toml";


    }

}
