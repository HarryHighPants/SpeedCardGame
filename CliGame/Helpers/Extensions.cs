namespace CliGame.Helpers;

using System.Text.RegularExpressions;

public static class Extensions
{
    public static int? ExtractInt(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        var stripped = Regex.Replace(
            input, // Our input
            "[^0-9]", // Select everything that is not in the range of 0-9
            ""
        ); // Replace that with an empty string.
        if (int.TryParse(stripped, out var number))
        {
            return number;
        }

        return null;
    }
}
