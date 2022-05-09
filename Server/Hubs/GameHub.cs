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

	private Guid UserIdentifier => Guid.Parse(Context.UserIdentifier ?? throw new InvalidOperationException("User not signed in"));

	public GameHub(IGameService gameService)
	{
		this.gameService = gameService;
	}

	public async Task JoinRoom(string roomId, BotType? botType = null)
	{
		Console.WriteLine($"Join room, {UserIdentifier}  room: {roomId}");

		// Add gameParticipant to the group with roomId
		await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

		// Add the player to the room
		gameService.JoinRoom(roomId, UserIdentifier, botType);

	}

	public async Task UpdateName(string roomId, string name)
	{
		gameService.UpdateName(name, UserIdentifier);
	}

	public async Task LeaveRoom(string roomId)
	{
		Console.WriteLine($"Remove gameParticipant from room id: {roomId}");

		// Remove gameParticipant to the group
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

		// Remove the gameParticipant from the room
		gameService.LeaveRoom(roomId, UserIdentifier);
	}

	public async Task StartGame(string roomId) => ThrowIfErrorResult(await gameService.StartGame(roomId, UserIdentifier));

	public async Task TryPlayCard(string roomId, int cardId, int centerPileId) => ThrowIfErrorResult(await gameService.TryPlayCard(roomId, UserIdentifier, cardId, centerPileId));

	public async Task TryPickupFromKitty(string roomId) => ThrowIfErrorResult(await gameService.TryPickupFromKitty(roomId, UserIdentifier));

	public async Task TryRequestTopUp(string roomId) => ThrowIfErrorResult(await gameService.TryRequestTopUp(roomId, UserIdentifier));

	public async Task UpdateMovingCard(string roomId, UpdateMovingCardData updateMovingCard)
	{
		//todo: create a card location service
		//todo: that potentially uses the gameService to determine the user owns that card

		// // Check gameParticipant owns the card
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

	//
	// public DailyResultDto? RequestDailyResults()
	// {
	// 	var dailyResult = statService.GetDailyResultDto(UserIdentifier);
	//
	// 	// Send the SendDailyResult to the player
	// 	return dailyResult;
	// }
	//
	// public async Task<RankingStatsDto> RequestEloInfo(GameParticipant gameParticipant, string roomId)
	// {
	// 	return await statService.GetRankingStats(gameParticipant.PersistentPlayerId, roomId);
	// }

	private void ThrowIfErrorResult(Result result)
	{
		if (result is IErrorResult resultError)
		{
			throw new HubException(resultError.Message);
		}
	}


}
