namespace Server.Services;

public interface IBotService
{
	public void AddBotToRoom(string roomId, BotType type);
	public int BotsInRoomCount(string roomId);
	public List<BotService.Bot> GetBotsInRoom(string roomId);
	public void RemoveBotsFromRoom(string roomId);
	public void RunBotsInRoom(string roomId);
}
