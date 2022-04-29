using System.Security.Cryptography;
using System.Text;
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
	private static WebBotData GetDailyBot()
	{
		// Add 10 hours for AEST
		var dayIndex = DateOnly.FromDateTime(DateTime.Today.AddHours(10)).DayNumber;

		var botName = BotNameCreator.GetRandomBotName(dayIndex);
		var random = new Random(dayIndex);
		var difficultyMultiplier = random.Next(50, 150) * 0.01;
		return new WebBotData
		{
			PersistentId = GuidFromString(dayIndex.ToString()),
			Name = botName,
			QuickestResponseTimeMs = (int)(1500 * difficultyMultiplier),
			SlowestResponseTimeMs = (int)(4000 * difficultyMultiplier),
			PickupIntervalMs = (int)(1000 * difficultyMultiplier),
			Elo = (int)(2500 * difficultyMultiplier)
		};
	}

	public static WebBotData GetBot(BotType type) =>
		type switch
		{
			BotType.Daily => GetDailyBot(),
			_ => BotConfigurations.Bots[type]
		};

	public static readonly Dictionary<BotType, WebBotData> Bots = new()
    {
        {
            BotType.Easy,
            new WebBotData
            {
	            PersistentId = new Guid("2dfc30e2-44a8-432b-b0fb-58f16159a2bd"),
                Name = "Limping Liam",
                CustomIntroMessage = "He can't jump far",
                CustomLoseMessage = "Oh no",
                CustomWinMessage = "Easy",
                QuickestResponseTimeMs = 4000,
                SlowestResponseTimeMs = 7000,
                PickupIntervalMs = 1500,
                Elo = 1000,
            }
        },
        {
            BotType.Medium,
            new WebBotData
            {
	            PersistentId = new Guid("6e03157b-876c-4b10-90b9-e8957e78aad2"),
                Name = "Harrowing Hayden",
                CustomIntroMessage = "He's a bit of a trickster so watch out",
                CustomLoseMessage = "Damn, he's tricky",
                CustomWinMessage = "Down goes the trickster",
                QuickestResponseTimeMs = 2000,
                SlowestResponseTimeMs = 5000,
                PickupIntervalMs = 1000,
                Elo = 2000,
            }
        },
        {
            BotType.Hard,
            new WebBotData
            {
	            PersistentId = new Guid("7e8539bd-7ec6-409a-abda-40974e025905"),
                Name = "Masterful Mikaela",
                CustomIntroMessage = "She can't be trusted",
                CustomLoseMessage = "Oof, rough one",
                CustomWinMessage = "Down falls Mikaela and her wicked ways",
                QuickestResponseTimeMs = 1000,
                SlowestResponseTimeMs = 3500,
                PickupIntervalMs = 750,
                Elo = 3000,
            }
        },
        {
            BotType.Impossible,
            new WebBotData
            {
	            PersistentId = new Guid("4acccae8-34f5-43cd-ac02-ed837da994f8"),
                Name = "Chaotic Kate",
                CustomIntroMessage = "rip lol",
                CustomLoseMessage = "No chance",
                CustomWinMessage = "No one will ever see this message so it doesn't matter",
                QuickestResponseTimeMs = 1000,
                SlowestResponseTimeMs = 2000,
                PickupIntervalMs = 500,
                Elo = 4000,
            }
        }
    };

	private static Guid GuidFromString(string input)
	{
		using (MD5 md5 = MD5.Create())
		{
			byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
			return new Guid(hash);
		}
	}
}
