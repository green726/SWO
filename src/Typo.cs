public static class Typo
{
    //set parameters
    private const int initialCapacity = 82765;
    private const int maxEditDistance = 2;
    private const int prefixLength = 7;
    private static SymSpell symSpell = new SymSpell(initialCapacity, maxEditDistance, prefixLength);

    public static void initialize()
    {
        string path = AppDomain.CurrentDomain.BaseDirectory + "../../../../src/typo-dictionary.txt"; //path referencing the SymSpell core project
        int termIndex = 0; //column of the term in the dictionary text file
        int countIndex = 1; //column of the term frequency in the dictionary text file
        if (!symSpell.LoadDictionary(path, termIndex, countIndex))
        {
            Console.WriteLine("File not found!");
            //press any key to exit program
            Console.ReadKey();
            return;
        }

    }

    public static List<string> spellCheck(string input)
    {
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

        return retList;
    }

}
