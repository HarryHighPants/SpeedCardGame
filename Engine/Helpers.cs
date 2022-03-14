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
}