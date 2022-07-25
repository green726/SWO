using System.Runtime.InteropServices;


public static class Util
{
    public static string[] delimeters = { "(", ")", "{", "}", "[", "]" };
    public static string[] builtinFuncs = { "print", "println" };
    // public static TokenType[] delimTypes = { TokenType.ParenDelimiterOpen, TokenType.ParenDelimiterClose, TokenType.BrackDelimiterOpen, TokenType.BrackDelimiterClose, TokenType.SquareDelimiterOpen, TokenType.SquareDelimiterClose };
    //
    //BUG: remove this in release builds or figure out a better way for HISS path
    public static string installPath = "";
    public static string pathToSrc = "";


    public static void getHISSInstallPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            pathToSrc = "/home/green726/coding/HISS/src/";
            installPath = Environment.ExpandEnvironmentVariables($"%HOME%/.HISS");
        }

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            pathToSrc = @"D:/coding/HISS/src/";
            installPath = Environment.ExpandEnvironmentVariables(@$"%USERPROFILE%/.HISS");
        }
    }

    public enum TokenType
    {
        Operator,
        Int,
        Double,
        Keyword,
        DelimiterClose,
        DelimiterOpen,
        AssignmentOp,
        String,
        Text,
        Special,
        EOL, //end of line,
        EOF //end of file
    }

    public static List<TokenType> allTokenTypesExcept(List<TokenType> exceptedTypes)
    {
        List<TokenType> ret = new List<TokenType>();
        foreach (TokenType type in Enum.GetValues(typeof(TokenType)))
        {
            if (exceptedTypes.Contains(type))
            {
                continue;
            }
            ret.Add(type);
        }

        return ret;
    }

    public enum ClassType
    {
        Double,
        Int,
        String
    }


    public class Token
    {
        public string value;
        public TokenType type;
        public int line;
        public int column;
        public bool isDelim = false;
        public int charNum = 0;

        public Token(TokenType type, string value, int line, int column, bool isDelim = false)
        {
            this.isDelim = isDelim;
            this.value = value;
            this.type = type;
            this.line = line;
            this.column = column;

            if (this.type == TokenType.String)
            {
                this.value = value.Substring(1);
                this.value = this.value.Substring(0, (this.value.Length - 1));
            }
        }

        public Token(TokenType type, char value, int line, int column, bool isDelim = false)
        {
            this.isDelim = isDelim;
            this.value = value.ToString();
            this.type = type;
            this.line = line;
            this.column = column;
        }

        public Token(TokenType type, char value, int line, int column, int charNum, bool isDelim = false)
        {
            this.isDelim = isDelim;
            this.value = value.ToString();
            this.type = type;
            this.line = line;
            this.column = column;
            this.charNum = charNum;
        }

        public Token(TokenType type, string value, int line, int column, int charNum, bool isDelim = false)
        {
            this.isDelim = isDelim;
            this.value = value.ToString();
            this.type = type;
            this.line = line;
            this.column = column;
            this.charNum = charNum;
        }

    }

    public static void copyDirectory(string sourceDir, string destinationDir, bool recursive, bool overwrite)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, overwrite);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                copyDirectory(subDir.FullName, newDestinationDir, true, overwrite);
            }
        }
    }

    public static string checkOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (RuntimeInformation.OSArchitecture.ToString() == "X64")
            {
                return "linux-x64";
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return RuntimeInformation.RuntimeIdentifier;
        }
        return "unknown";
    }


}

