namespace Server.Services;

using Engine;

public record WebBotData : BotData
{
    public Guid PersistentId;
    public BotType Type;
    public double Elo;
}
