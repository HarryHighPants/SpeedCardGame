namespace Server.Hubs;

using Engine.Helpers;
using Engine.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Models.Database;
using Newtonsoft.Json;
using Services;

[Authorize]
public class GameHub : Hub
{
	private readonly IGameService gameService;
	private readonly IHubContext<GameHub> hubContext;
	private readonly IBotService botService;
	private readonly GameResultContext gameResultContext;
	private readonly StatService statService;

	private Guid UserIdentifier => Guid.Parse(Context.UserIdentifier ?? throw new InvalidOperationException("User not signed in"));

	public GameHub(GameResultContext gameResultContext, IGameService gameService, IHubContext<GameHub> hubContext, StatService statService,
		IBotService botService)
	{
		this.gameResultContext = gameResultContext;
		this.gameService = gameService;
		this.hubContext = hubContext;
		this.botService = botService;
		this.statService = statService;
	}

	public async Task JoinRoom(string roomId, BotType? botType = null)
	{
		Console.WriteLine($"Join room, {UserIdentifier}  room: {roomId}");

		// Add connection to the group with roomId
		await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

		// Add the player to the room
		gameService.JoinRoom(roomId, UserIdentifier, botType);

		// Send gameState for roomId
		await SendGameState(roomId);
		await SendLobbyState(roomId);
	}

	public async Task UpdateName(string roomId, string name)
	{
		gameService.UpdateName(name, UserIdentifier);
		await SendLobbyState(roomId);
	}

	public async Task LeaveRoom(string roomId)
	{
		Console.WriteLine($"Remove connection from room id: {roomId}");

		// Remove connection to the group
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

		// Remove the connection from the room
		gameService.LeaveRoom(roomId, UserIdentifier);

		// Send gameState for roomId
		await SendGameState(roomId);
		await SendLobbyState(roomId);
	}

	public async Task StartGame(string roomId)
	{
		var startGameResult = gameService.StartGame(roomId, UserIdentifier);
		if (startGameResult is IErrorResult startGameError)
		{
			await SendConnectionMessage(startGameError.Message);
			throw new HubException(startGameError.Message, new UnauthorizedAccessException(startGameError.Message));
		}

		await SendLobbyState(roomId);
		await SendGameState(roomId);
	}

	public async Task TryPlayCard(string roomId, int cardId, int centerPileId)
	{
		var tryPlayCardResult = gameService.TryPlayCard(roomId, UserIdentifier, cardId, centerPileId);
		if (tryPlayCardResult is IErrorResult tryPlayCardResultError)
		{
			await SendConnectionMessage(tryPlayCardResultError.Message);
			throw new HubException(tryPlayCardResultError.Message,
				new UnauthorizedAccessException(tryPlayCardResultError.Message));
		}

		await SendGameState(roomId);
	}

	public async Task TryPickupFromKitty(string roomId)
	{
		var pickupResult = gameService.TryPickupFromKitty(roomId, UserIdentifier);
		await SendGameState(roomId);
		// todo: add helper method to handle IErrorResult
		if (pickupResult is IErrorResult pickupResultError)
		{
			await SendConnectionMessage(pickupResultError.Message);
			throw new HubException(pickupResultError.Message,
				new UnauthorizedAccessException(pickupResultError.Message));
		}
	}

	public async Task TryRequestTopUp(string roomId)
	{
		var topUpResult = gameService.TryRequestTopUp(roomId, UserIdentifier);
		await SendGameState(roomId);
		if (topUpResult is IErrorResult topUpResultError)
		{
			await SendConnectionMessage(topUpResultError.Message);
			throw new HubException(topUpResultError.Message, new UnauthorizedAccessException(topUpResultError.Message));
		}
	}

	public async Task UpdateMovingCard(string roomId, UpdateMovingCardData updateMovingCard)
	{
		//todo: create a card location service
		//todo: that potentially uses the gameService to determine the user owns that card

		// // Check connection owns the card
		// var cardLocation = gameService.GetCardLocation(UserIdentifier, updateMovingCard.CardId);
		// if (cardLocation == null)
		// {
		// 	await SendConnectionMessage("No card found with that Id");
		// 	throw new HubException("No card found with that Id",
		// 		new UnauthorizedAccessException("No card found with that Id"));
		// }
		//
		// var authorisedUpdate = updateMovingCard;
		// var authorised = gameService.ConnectionOwnsCard(UserIdentifier, updateMovingCard.CardId);
		// if (!authorised)
		// {
		// 	authorisedUpdate = authorisedUpdate with {Pos = null};
		// }
		//
		// authorisedUpdate.Location = (int)cardLocation.Value.PileName;
		// authorisedUpdate.Index = cardLocation.Value.PileName == CardPileName.Hand
		// 	? cardLocation.Value.PileIndex ?? -1
		// 	: cardLocation.Value.CenterIndex ?? -1;
		//
		// // Send update to the others in the group
		// var roomId = gameService.GetConnectionsRoomId(UserIdentifier);
		// var jsonData = JsonConvert.SerializeObject(authorisedUpdate);
		// await Clients.OthersInGroup(roomId).SendAsync("MovingCardUpdated", jsonData);
	}


	private async Task SendGameState(string roomId)
	{
		// Get the gameState from the gameService
		var gameStateResult = gameService.GetGameStateDto(roomId);

		// If there is no valid game state, there is nothing to send
		if (gameStateResult is IErrorResult)
		{
			return;
		}

		// Send the gameState to the roomId
		var jsonData = JsonConvert.SerializeObject(gameStateResult.Data);
		await hubContext.Clients.Group(roomId).SendAsync("UpdateGameState", jsonData);
	}

	private async Task SendLobbyState(string roomId)
	{
		// Get the gameState from the gameService
		var lobbyStateResult = gameService.GetLobbyStateDto(roomId);

		// Check it's valid
		if (lobbyStateResult is IErrorResult lobbyStateError)
		{
			throw new HubException(lobbyStateError.Message, new ApplicationException(lobbyStateError.Message));
		}

		// Send the gameState to the roomId
		var jsonData = JsonConvert.SerializeObject(lobbyStateResult.Data);
		await Clients.Group(roomId).SendAsync("UpdateLobbyState", jsonData);
	}

	public DailyResultDto? RequestDailyResults()
	{
		var dailyResult = statService.GetDailyResultDto(UserIdentifier);

		// Send the SendDailyResult to the player
		return dailyResult;
	}

	public async Task<RankingStatsDto> RequestEloInfo(Connection connection, string roomId)
	{
		return await statService.GetRankingStats(connection.PersistentPlayerId, roomId);
	}

	private async Task SendConnectionMessage(string message) =>
		await Clients.Client(Context.UserIdentifier).SendAsync("Message", message);
}
