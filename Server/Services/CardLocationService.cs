using Engine.Helpers;
using Engine.Models;
using Microsoft.AspNetCore.SignalR;
using Server.Helpers;

namespace Server.Services;

public class CardLocationService
{
    private readonly IGameService gameService;

    public CardLocationService(IGameService gameService)
    {
        this.gameService = gameService;
    }

    /// <summary>
    /// Updating a cards co-ordinates from the client if they have permission
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="persistentPlayerId"></param>
    /// <param name="updateMovingCard"></param>
    /// <exception cref="Exception"></exception>
    /// <exception cref="HubException"></exception>
    public async Task<UpdateMovingCardData> UpdateMovingCard(string roomId, Guid persistentPlayerId, UpdateMovingCardData updateMovingCard)
    {
        var playerIdHash = persistentPlayerId.ToString().Hash();
        var gameStateResult = await gameService.GetGameState(roomId);
        if (gameStateResult.Failure)
        {
            throw new Exception("Error getting game state");
        }
        var gameState = gameStateResult.Data;
        
        // We need to check the card they are trying to update belongs to the player and is in the most updated location
        var cardLocation = GetPlayerCardLocation(playerIdHash, updateMovingCard.CardId, gameState);
        if (cardLocation == null)
        {
            updateMovingCard = updateMovingCard with {Pos = null};
        }

        return updateMovingCard;
    }

    private CardPileName? GetPlayerCardLocation(string playerIdHash, int cardId, GameStateDto gameState)
    {
        var player = gameState.Players.SingleOrDefault(p => p.IdHash == playerIdHash);
        if (player == null)
        {
            return null;
        }

        if (player.HandCards.Any(c => c.Id == cardId))
        {
            return CardPileName.Hand;
        }

        if (player.TopKittyCardId == cardId)
        {
            return CardPileName.Kitty;
        }

        return null;
    }
}