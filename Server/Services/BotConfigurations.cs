using Engine;
using Server.Services;

public enum BotType
{
    Easy,
    Medium,
    Hard,
    Impossible,
    Daily
}

public class BotConfigurations
{
	private static BotData GetDailyBot()
	{
		// Add 10 hours for AEST
		var dayIndex = DateTime.Today.AddHours(10).Subtract(DateTime.Parse("12-April-2022")).Days;
		Console.WriteLine($"Get daily bot index: {dayIndex} today: {DateTime.Today.AddHours(10).ToString()} {DateTime.Parse("12-April-2022").ToString()}");
		var botName = BotNameCreator.GetRandomBotName(dayIndex);
		var random = new Random(dayIndex);
		var difficultyMultiplier = random.Next(50, 150) * 0.01;
		return new BotData
		{
			Name = botName,
			QuickestResponseTimeMs = (int)(1500 * difficultyMultiplier),
			SlowestResponseTimeMs = (int)(4000 * difficultyMultiplier),
			PickupIntervalMs = (int)(1000 * difficultyMultiplier),
		};
	}


	public static BotData GetBot(BotType type) =>
		type switch
		{
			BotType.Daily => GetDailyBot(),
			_ => BotConfigurations.Bots[type]
		};

	private static readonly Dictionary<BotType, BotData> Bots = new()
    {
        {
            BotType.Easy,
            new BotData
            {
                Name = "Limping Liam",
                CustomIntroMessage = "He can't jump far",
                CustomLoseMessage = "Oh no",
                CustomWinMessage = "Easy",
                QuickestResponseTimeMs = 4000,
                SlowestResponseTimeMs = 7000,
                PickupIntervalMs = 1500
            }
        },
        {
            BotType.Medium,
            new BotData
            {
                Name = "Harrowing Hayden",
                CustomIntroMessage = "He's a bit of a trickster so watch out",
                CustomLoseMessage = "Damn, he's tricky",
                CustomWinMessage = "Down goes the trickster",
                QuickestResponseTimeMs = 2000,
                SlowestResponseTimeMs = 5000,
                PickupIntervalMs = 1000
            }
        },
        {
            BotType.Hard,
            new BotData
            {
                Name = "Masterful Mikaela",
                CustomIntroMessage = "She can't be trusted",
                CustomLoseMessage = "Oof, rough one",
                CustomWinMessage = "Down falls Mikaela and her wicked ways",
                QuickestResponseTimeMs = 1000,
                SlowestResponseTimeMs = 4000,
                PickupIntervalMs = 750
            }
        },
        {
            BotType.Impossible,
            new BotData
            {
                Name = "Chaotic Kate",
                CustomIntroMessage = "rip lol",
                CustomLoseMessage = "No chance",
                CustomWinMessage = "No one will ever see this message so it doesn't matter",
                QuickestResponseTimeMs = 1000,
                SlowestResponseTimeMs = 2500,
                PickupIntervalMs = 500
            }
        }
    };
}
