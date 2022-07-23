//NOTE: big refers to the default symSpell that checks a good portion of the english language 
//NOTE: little refers to the HISS symSpell that checks HISS keywords + currently declared vars and funcs etc

public static class Typo
{
    //set parameters
    private const int bigInitialCapacity = 82765;
    private const int bigMaxEditDistance = 2;
    private const int bigPrefixLength = 7;
    private static SymSpell bigSym = new SymSpell(bigInitialCapacity, bigMaxEditDistance, bigPrefixLength);

    private const int littleInitialCapacity = 0;
    private const int littleMaxEditDistance = 2;
    private const int littlePrefixLength = 7;
    private static SymSpell littleSym = new SymSpell(littleInitialCapacity, littleMaxEditDistance, littlePrefixLength);

    private static string bigPath = AppDomain.CurrentDomain.BaseDirectory + "./src/Dictionaries/typo-dictionary.txt";
    private static string littlePath = AppDomain.CurrentDomain.BaseDirectory + "./src/Dictionaries/hiss-dictionary.txt";
    private static int termIndex = 0; //column of the term in the dictionary text file
    private static int countIndex = 1; //column of the term frequency in the dictionary text file

    private static string littleDictDefaults = @"print! 1
print 1
println! 1
println 1
main 1
@main 1
var 1
const 1
int 1
double 1
string 1
for 1
if 1
else 1
in 1
";

    private static StreamWriter littleWriter;

    private static Stream fileStream;

    public static void initialize()
    {
        loadDict(Checker.Little);
        loadDict(Checker.Big);
        // foreach (string line in littleDictDefaults)
        // {
        //     littleWriter.Write(line);
        // }

        File.Delete(littlePath);
        fileStream = new FileStream(littlePath, FileMode.Append);
        littleWriter = new StreamWriter(fileStream);
        littleWriter.Write(littleDictDefaults);
        // littleWriter.Flush();
        littleWriter.Close();
    }

    public static List<string> spellCheck(string input)
    {
        List<string> retList = new List<string>();

        const SymSpell.Verbosity verbosity = SymSpell.Verbosity.Closest;
        List<SymSpell.SuggestItem> littleSuggestions = littleSym.Lookup(input, verbosity);
        List<SymSpell.SuggestItem> bigSuggestions;

        foreach (SymSpell.SuggestItem suggestion in littleSuggestions)
        {
            retList.Add(suggestion.term);
            // Console.WriteLine($"little suggestion: {suggestion.term}");
            if (retList.Count() > 3)
            {
                break;
            }
        }

        if (retList.Count() == 0)
        {
            bigSuggestions = bigSym.Lookup(input, verbosity);
            foreach (SymSpell.SuggestItem suggestion in bigSuggestions)
            {
                if (!retList.Contains(suggestion.term))
                {
                    retList.Add(suggestion.term);
                }
                if (retList.Count() >= 3)
                {
                    break;
                }
            }
        }


        return retList;
    }

    public static void loadDict(Checker checker)
    {
        if (checker == Checker.Big)
        {
            if (!bigSym.LoadDictionary(bigPath, termIndex, countIndex))
            {
                Console.WriteLine("Big dictionary not found!");
                //press any key to exit program
                Console.ReadKey();
                return;
            }
        }
        else
        {
            if (!littleSym.LoadDictionary(littlePath, termIndex, countIndex))
            {
                Console.WriteLine("Little dictionary not found!");
                //press any key to exit program
                Console.ReadKey();
                return;
            }
        }
    }

    public static void addToLittle(AST.Node node)
    {

        fileStream = new FileStream(littlePath, FileMode.Append);
        littleWriter = new StreamWriter(fileStream);
        switch (node.nodeType)
        {
            case AST.Node.NodeType.VariableAssignment:
                AST.VariableAssignment varAss = (AST.VariableAssignment)node;
                littleWriter.WriteLine($"{varAss.name} 1");
                // Console.WriteLine($"added var ass with name {varAss.name} to little dict");
                break;
        }
        littleWriter.Close();
        loadDict(Checker.Little);
    }

    public enum Checker
    {
        Big,
        Little
    }

}
