namespace AST;
using System.Linq;

public class ImportStatement : Node
{
    public HISSFile file;
    public Library library;

    public ImportStatement(Util.Token token) : base(token)
    {

    }

    public override void addChild(Node child)
    {
        base.addChild(child);
    }

    public override void addChild(Util.Token child)
    {
        Console.WriteLine("adding token child to import");
        List<Library> matchingLib = (List<Library>)HISS.projectInfo.libraries.Where(library => library.name == child.value);
        if (matchingLib.Count() == 1)
        {
            this.library = matchingLib[0];
        }
        else if (matchingLib.Count() > 1)
        {
            throw new ParserException("Multiple libraries found matching the same name - possible HIP bug or incorrect using statement", this);
        }
        else
        {
            string[] filePaths = Directory.GetFiles(HISS.projectInfo.path, child.value + ".hiss");

            if (filePaths.Length > 1)
            {
                throw new ParserException("Multiple files found matching the same name in using statement", this);
            }
            else if (filePaths.Length == 0)
            {
                throw ParserException.FactoryMethod("No file or library found that matches using statement", "Remove incorrect using statement - fix a possible typo", this, true);
            }

            this.file = new HISSFile(child.value, filePaths[0]);
        }

        base.addChild(child);
    }
}
