namespace AST;
using System.Linq;

public class ImportStatement : Node
{
    public SWOFile file;
    public Library library;

    public ImportStatement(Util.Token token) : base(token)
    {
        this.nodeType = NodeType.ImportStatement;
        this.newLineReset = true;
    }

    public override void addChild(Node child)
    {
        base.addChild(child);
    }

    public string[] getASTFilePaths(string dir, string name)
    {
        string[] filePaths = Directory.GetFiles(SWO.projectInfo.path, name);
        return filePaths;
    }

    public bool importAST(string childVal)
    {
        string[] astPaths = getASTFilePaths(SWO.projectInfo.path, childVal + ".ast.json");
        if (astPaths.Length == 0)
        {
            return false;
            // throw ParserException.FactoryMethod("No file or library found that matches using statement", "Remove incorrect using statement - fix a possible typo", this, true);
        }
        ASTFile file = ASTFile.deserialize(astPaths[0]);

        foreach (var proto in file.prototypes)
        {
            this.parser.declaredFuncs.Add(proto.Key, proto.Value);
        }

        foreach (var str in file.structs)
        {
            this.parser.declaredStructs.Add(str.Key, str.Value);
        }
        return true;

    }

    public void importSource(string childVal)
    {
        if (Config.settings.general.import.favorAST)
        {
            if (importAST(childVal) == true)
            {
                return;
            }
        }
        string[] filePaths = Directory.GetFiles(SWO.projectInfo.path, childVal + ".swo");

        if (filePaths.Length > 1)
        {
            throw new ParserException("Multiple files found matching the same name in using statement", this);
        }
        else if (filePaths.Length == 0 && !Config.settings.general.import.favorAST)
        {
            importAST(childVal);
            return;
        }

        this.file = new SWOFile(childVal, filePaths[0]);

        string importFileValue = this.file.value;
        List<Util.Token> tokens = Lexer.lex(importFileValue);

        // Parser.finalTokenNum += tokens.Count();
        // Parser.tokenList.InsertRange(Parser.currentTokenNum, tokens);
        // List<Util.Token> prevList = Parser.tokenList;
        // int prevNum = Parser.currentTokenNum;
        // int prevMaxNum = Parser.finalTokenNum;
        //
        // Parser.parse(tokens);
        //
        // Parser.currentTokenNum = prevNum;
        // Parser.tokenList = prevList;
        // Parser.finalTokenNum = prevMaxNum;
        //
        //TODO: implement imports

        Parser.addInstance(tokens, this.file.name, this.file.path);
    }

    public override void addChild(Util.Token child)
    {
        base.addChild(child);
        List<Library> matchingLib = SWO.projectInfo.libraries.Where(library => library.name == child.value).ToList();
        if (matchingLib.Count() == 1)
        {
            this.library = matchingLib[0];
        }
        else if (matchingLib.Count() > 1)
        {
            throw new ParserException("Multiple libraries found matching the same name - possible SAP bug or incorrect using statement", this);
        }
        else
        {
            importSource(child.value);
        }


    }
}
