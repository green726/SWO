namespace TranspilerGenerator;

public class TranspilerGen
{

    public static Stack<TranspilerGen> generatorStack = new Stack<TranspilerGen>();

    public static TranspilerGen getInstance()
    {
        return generatorStack.Peek();
    }

    public static TranspilerGen addInstance(Parser parser, string fileName, TranspileCommandSettings settings)
    {
        TranspilerGen newGen = new TranspilerGen(parser, fileName, settings);
        generatorStack.Push(newGen);
        return newGen;
    }

    public static TranspilerGen removeInstance()
    {
        return generatorStack.Pop();
    }

    public TranspilerGen(Parser parser, string fileName, TranspileCommandSettings settings)
    {
        DebugConsole.WriteAnsi($"[blue]fileName: {fileName}[/]");
        this.fileName = fileName;
        this.parser = parser;
        this.fileInfo = new FileInfo(fileName);
        this.settings = settings;
    }

    public Parser parser;

    public TranspileCommandSettings settings;

    public string fileName = "";
    public FileInfo fileInfo;

    private AST.Node.NodeType[] noSemiColonNodes = {AST.Node.NodeType.ForLoop, AST.Node.NodeType.IfStatement, AST.Node.NodeType.ElseStatement, AST.Node.NodeType.ElseIfStatement};

    //just trust bro its more efficient
    public string file = "";

    public void write(String text)
    {
        file += text;
    }

    public void writeLine()
    {
        file += "\n";
    }

    public void writeLine(String text)
    {
        file += text + "\n";
    }

    private void actuallyWrite(string filePath)
    {
        string finalName = fileName.Substring(0, fileName.Length - 4) + "-transpiled";
        File.WriteAllText($"{filePath}/{finalName}.swo", file);
    }

    public void checkSemiColon(AST.Node node) {
        if (!noSemiColonNodes.Contains(node.nodeType)) {
            this.write(";");
        }
    }

    public void checkSemiColonAndNL(AST.Node node) {
        if (!noSemiColonNodes.Contains(node.nodeType)) {
            this.writeLine(";");
        }
    }

    public void generate()
    {
        foreach (AST.Node node in parser.nodes)
        {
            node.transpilerGenerator.generate();
        }
        actuallyWrite(settings.path);
    }

}
