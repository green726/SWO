public static class Typo
{
    //set parameters
    private const int initialCapacity = 82765;
    private const int maxEditDistance = 2;
    private const int prefixLength = 7;
    private static SymSpell symSpell = new SymSpell(initialCapacity, maxEditDistance, prefixLength);

    private static string bigPath = AppDomain.CurrentDomain.BaseDirectory + "../../../../src/Dictionaries/typo-dictionary.txt";
    private static string littlePath = AppDomain.CurrentDomain.BaseDirectory + "../../../../src/Dictionaries/hiss-dictionary.txt";
    private static int termIndex = 0; //column of the term in the dictionary text file
    private static int countIndex = 1; //column of the term frequency in the dictionary text file

    public static List<string> spellCheck(string input, bool loadLittle = true)
    {
        if (loadLittle)
        {
            loadDict(littlePath);
        }
        else
        {
            loadDict(bigPath);
        }

        List<string> retList = new List<string>();

        const SymSpell.Verbosity verbosity = SymSpell.Verbosity.Closest;
        List<SymSpell.SuggestItem> suggestions = symSpell.Lookup(input, verbosity);

        foreach (SymSpell.SuggestItem suggestion in suggestions)
        {
            retList.Add(suggestion.term);
            if (retList.Count() > 4)
            {
                break;
            }
        }

        if (retList.Count < 1)
        {
            return spellCheck(input, false);
        }


        return retList;
    }

    public static void loadDict(string path)
    {
        if (!symSpell.LoadDictionary(path, termIndex, countIndex))
        {
            Console.WriteLine("File not found!");
            //press any key to exit program
            Console.ReadKey();
            return;
        }
    }

}
