using System.Security.Cryptography;
using System.Text;
using Engine;
using Server.Hubs;
using Server.Services;

public class BotConfigurations
{
	private static WebBotData GetDailyBot()
	{
		var dayIndex = GameHub.GetDayIndex();

		var botName = BotNameCreator.GetRandomBotName(dayIndex);
		var random = new Random(dayIndex);
		var difficultyMultiplier = random.Next(50, 150) * 0.01;
		var botData = GetBot(BotType.Medium);
		botData = botData with
		{
			Name = botName,
			PersistentId = GuidFromString(dayIndex.ToString()),
			QuickestResponseTimeMs = botData.QuickestResponseTimeMs * difficultyMultiplier,
			SlowestResponseTimeMs = botData.SlowestResponseTimeMs * difficultyMultiplier,
			PickupIntervalMs = botData.PickupIntervalMs * difficultyMultiplier,
			Elo = botData.Elo / difficultyMultiplier,
			Type = BotType.Daily,
		};
		return botData;
	}

	public static WebBotData GetBot(BotType type) =>
		type switch
		{
			BotType.Daily => GetDailyBot(),
			_ => Bots[type]
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
                QuickestResponseTimeMs = 2000,
                SlowestResponseTimeMs = 4000,
                PickupIntervalMs = 1000,
                Elo = 1500,
                Type = BotType.Easy
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
                QuickestResponseTimeMs = 1000,
                SlowestResponseTimeMs = 3000,
                PickupIntervalMs = 500,
                Elo = 2500,
                Type = BotType.Medium
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
                QuickestResponseTimeMs = 500,
                SlowestResponseTimeMs = 1750,
                PickupIntervalMs = 400,
                Elo = 3500,
                Type = BotType.Hard
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
                QuickestResponseTimeMs = 500,
                SlowestResponseTimeMs = 750,
                PickupIntervalMs = 300,
                Elo = 4500,
                Type = BotType.Impossible
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
