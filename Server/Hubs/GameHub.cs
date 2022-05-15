namespace Server.Hubs;

using Engine.Helpers;
using Engine.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Models.Database;
using Newtonsoft.Json;
using Services;

public interface IGameHubClient
{
    public Task UpdateGameState(GameStateDto gameStateDto);
    public Task UpdateLobbyState(LobbyStateDto lobbyStateDto);
    public Task UpdateMovingCard(UpdateMovingCardData movingCard);
    public Task ErrorMessage(string message);
}

[Authorize]
public class GameHub : Hub<IGameHubClient>
{
    private readonly IGameService gameService;
    private readonly CardLocationService cardLocationService;

    private Guid UserIdentifier =>
        Guid.Parse(
            Context.UserIdentifier ?? throw new InvalidOperationException("User not signed in")
        );

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await gameService.LeaveAllRooms(UserIdentifier);
        await base.OnDisconnectedAsync(exception);
    }

    public GameHub(IGameService gameService, CardLocationService cardLocationService)
    {
        this.gameService = gameService;
        this.cardLocationService = cardLocationService;
    }

    public async Task JoinRoom(string roomId, BotType? botType = null)
    {
        Console.WriteLine($"Join room, {UserIdentifier}  room: {roomId}");

        // Add gameParticipant to the group with roomId
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        // Add the player to the room
        await gameService.JoinRoom(roomId, UserIdentifier, botType);
    }

    public async Task UpdateName(string roomId, string name)
    {
        await gameService.UpdateName(roomId, name, UserIdentifier);
    }

    public async Task LeaveRoom(string roomId)
    {
        Console.WriteLine($"Remove gameParticipant from room id: {roomId}");

        // Remove gameParticipant to the group
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

        // Remove the gameParticipant from the room
        await gameService.LeaveRoom(roomId, UserIdentifier);
    }

    public async Task StartGame(string roomId) =>
        await HandleErrorResult(await gameService.StartGame(roomId, UserIdentifier));

    public async Task TryPlayCard(string roomId, int cardId, int centerPileId) =>
        await HandleErrorResult(await gameService.TryPlayCard(roomId, UserIdentifier, cardId, centerPileId));

    public async Task TryPickupFromKitty(string roomId) =>
        await HandleErrorResult(await gameService.TryPickupFromKitty(roomId, UserIdentifier));

    public async Task TryRequestTopUp(string roomId) =>
        await HandleErrorResult(await gameService.TryRequestTopUp(roomId, UserIdentifier));

    public async Task UpdateMovingCard(string roomId, UpdateMovingCardData updateMovingCard)
    {
        var movingCard = await cardLocationService.UpdateMovingCard(roomId, UserIdentifier, updateMovingCard);
        // Send update to the others in the group
        await Clients.OthersInGroup(roomId).UpdateMovingCard(movingCard);
    }

    private async Task HandleErrorResult(Result result)
    {
        if (result is IErrorResult resultError)
        {
            await Clients.Caller.ErrorMessage(resultError.Message);
            Console.WriteLine(resultError.Message);
        }
    }
}