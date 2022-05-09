namespace Server;

using Engine.Models;

public record PlayerDto
{
    public string Id { get; init; }
    public string Name { get; init; } = "";
    public List<Card> HandCards { get; init; }
    public int? TopKittyCardId { get; init; }
    public int KittyCardsCount { get; init; }
    public bool RequestingTopUp { get; init; }
    public bool CanRequestTopUp { get; init; }
    public string LastMove { get; init; } = "";

    public PlayerDto(Player player, string playerId)
    {
        Id = playerId;
        Name = player.Name;
        HandCards = player.HandCards.ToList();
        KittyCardsCount = player.KittyCards.Count;
        RequestingTopUp = player.RequestingTopUp;
        TopKittyCardId = player.KittyCards.Count >= 1 ? player.KittyCards.Last().Id : null;
        CanRequestTopUp = player.CanRequestTopUp;
        LastMove = player.LastMove;
    }
}
