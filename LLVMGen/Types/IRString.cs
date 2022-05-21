using System.Linq;

public class IRString {
    public string value;

    public char[] charArray;
    public int[] intArray;
 
    public IRString(string value) {
        this.value = value;
        
        this.charArray = this.value.ToCharArray();
        intArray = this.charArray.Select(ch => (int)ch).ToArray();
    }

    

}
