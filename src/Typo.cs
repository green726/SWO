//NOTE: big refers to the default symSpell that checks a good portion of the english language 
//NOTE: little refers to the HISS symSpell that checks HISS keywords + currently declared vars and funcs etc

public static class Typo
{
    //set parameters
    private const int bigInitialCapacity = 82765;
    private const int bigMaxEditDistance = 2;
    private const int bigPrefixLength = 7;
    private static SymSpell bigSym = new SymSpell(bigInitialCapacity, bigMaxEditDistance, bigPrefixLength);

    private const int littleInitialCapacity = 82765;
    private const int littleMaxEditDistance = 2;
    private const int littlePrefixLength = 7;
    private static SymSpell littleSym = new SymSpell(littleInitialCapacity, littleMaxEditDistance, littlePrefixLength);

    private static string bigPath = AppDomain.CurrentDomain.BaseDirectory + "../../../../src/Dictionaries/typo-dictionary.txt";
    private static string littlePath = AppDomain.CurrentDomain.BaseDirectory + "../../../../src/Dictionaries/hiss-dictionary.txt";
    private static int termIndex = 0; //column of the term in the dictionary text file
    private static int countIndex = 1; //column of the term frequency in the dictionary text file

    public static List<string> spellCheck(string input)
    {
        List<string> retList = new List<string>();

        const SymSpell.Verbosity verbosity = SymSpell.Verbosity.Closest;
        List<SymSpell.SuggestItem> littleSuggestions = littleSym.Lookup(input, verbosity);
        List<SymSpell.SuggestItem> bigSuggestions;

        foreach (SymSpell.SuggestItem suggestion in littleSuggestions)
        {
            retList.Add(suggestion.term);
            if (retList.Count() > 4)
            {
                break;
            }
        }

        if (retList.Count < 1)
        {
            bigSuggestions = bigSym.Lookup(input, verbosity);
            retList = new List<string>();
            foreach (SymSpell.SuggestItem suggestion in bigSuggestions)
            {
                retList.Add(suggestion.term);
                if (retList.Count() > 4)
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

    public enum Checker
    {
        Big,
        Little
    }

}
