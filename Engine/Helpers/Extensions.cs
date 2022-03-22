using System.Collections.Immutable;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using static System.Int32;

namespace Engine.Helpers;

public static class Extensions
{
    public static void Shuffle<T>(this IList<T> list, int? randomSeed = null)
    {
        var random = new Random(randomSeed ?? Guid.NewGuid().GetHashCode());
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public static List<T> PopRange<T>(this List<T> stack, int amount)
    {
        var result = new List<T>(amount);
        while (amount-- > 0 && stack.Count > 0)
        {
            result.Add(stack[stack.Count - 1]);
            stack.RemoveAt(stack.Count - 1);
        }

        return result;
    }
    
    public static (ImmutableList<T> poppedList, ImmutableList<T> poppedItems) PopRange<T>(this ImmutableList<T> poppedList, int amount)
    {
        var poppedItems = new List<T>(amount);
        while (amount-- > 0 && poppedList.Count > 0)
        {
            poppedItems.Add(poppedList[^1]);
            poppedList = poppedList.RemoveAt(poppedList.Count - 1);
        }

        return (poppedList, poppedItems.ToImmutableList());
    }

    public static T Pop<T>(this List<T> stack)
    {
        T poppedItem = stack[stack.Count - 1];
        stack.RemoveAt(stack.Count - 1);
        return poppedItem;
    }

    public static int? ExtractInt(this string input)
    {
        if (string.IsNullOrEmpty(input)) return null;

        string stripped = Regex.Replace(
            input, // Our input
            "[^0-9]", // Select everything that is not in the range of 0-9
            ""); // Replace that with an empty string.
        if (TryParse(stripped, out int number))
        {
            return number;
        }

        return null;
    }
    
    public static T DeepClone<T>(this T obj)
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T) formatter.Deserialize(ms);
        }
    }

    public static IEnumerable<T> ReplaceElementAt<T>(this IEnumerable<T> source, int index, T element)
    {
        var i = 0;
        foreach (var item in source)
        {
            if (i == index)
            {
                yield return element;
            }
            else
            {
                yield return item;
            }

            i++;
        }
    }

    public static IEnumerable<(T item, int index)> IndexTuples<T>(this IEnumerable<T> source)
    {
        return source.Select((x, i) => (x, i));
    }
}