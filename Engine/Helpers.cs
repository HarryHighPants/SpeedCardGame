using System.Text.RegularExpressions;
using static System.Int32;

namespace Engine;

public static class Helpers
{
    private static Random rng = new Random();  
    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            (list[k], list[n]) = (list[n], list[k]);
        }  
    }
    
    public static List<T> PopRange<T>(this List<T> stack, int amount)
    {
        var result = new List<T>(amount);
        while (amount-- > 0 && stack.Count > 0)
        {
            result.Add(stack[stack.Count-1]);
            stack.RemoveAt(stack.Count-1);
        }
        return result;
    }

    public static int? ExtractInt(this string input)
    {
        var stripped = Regex.Replace(
            input, // Our input
            "[^0-9]", // Select everything that is not in the range of 0-9
            "");  // Replace that with an empty string.
        if(TryParse(stripped, out int number))
        {
            return number;
        } else
        {
            return null;
        }
    }
}