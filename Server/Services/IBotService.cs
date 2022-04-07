namespace Server.Services;

public interface IBotService
{
	public void AddBotToRoom(string roomId, BotDifficulty difficulty);
	public int BotsInRoomCount(string roomId);
	public void RemoveBotsFromRoom(string roomId);
	public void RunBotsInRoom(string roomId);
}
