namespace Server.Services;

using System.Linq;
using Newtonsoft.Json;

public static class BotNameCreator
{
	public static string GetRandomBotName(int seed)
	{
		var random = new Random(seed);
		for (var i = 0; i < 100000; i++)
		{
			var name = TryGetBotName(random);
			if (!string.IsNullOrEmpty(name))
			{
				return name;
			}
		}

		return "Hazardous Harry";
	}

	private static string? TryGetBotName(Random random)
	{
		var adjectivesJsonPath = Path.Combine(Environment.CurrentDirectory, "Assets", "Adjectives.json");
		var adjectivesJson = File.ReadAllText(adjectivesJsonPath);
		var adjectives = JsonConvert.DeserializeObject<List<string>>(adjectivesJson);
		var adjective = adjectives[random.Next(0, adjectives.Count)];

		var firstNamesJsonPath = Path.Combine(Environment.CurrentDirectory, "Assets", "FirstNames.json");
		var firstNamesJson = File.ReadAllText(firstNamesJsonPath);
		var firstNames = JsonConvert.DeserializeObject<List<string>>(firstNamesJson);
		var possibleFirstNames = firstNames.Where(name => name.Substring(0, 2) == adjective.Substring(0, 2)).ToList();
		if (possibleFirstNames.Count < 1)
		{
			return null;
		}
		var firstName = possibleFirstNames[random.Next(0, possibleFirstNames.Count)] ;
		return $"{adjective} {firstName}";
	}
}
