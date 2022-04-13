import { ICard } from './ICard'
import { IPlayer } from './IPlayer'

export interface IGameState {
    Players: IPlayer[]
    CenterPiles: CenterPile[]
    LastMove: string
	WinnerId?: string
}

export interface CenterPile {
	Cards: ICard[]
}

export type GameStateReducerAction =
	| {
	type: 'Play'
	topCard: ICard
	centerPileIndex: number
	playerId: string | null | undefined
}
	| {
	type: 'Pickup'
	playerId: string | null | undefined
}
	| {
	type: 'Replace'
	gameState: IGameState
}
export const gameStateReducer = (state: IGameState, action: GameStateReducerAction): IGameState => {
	switch (action.type) {
		case 'Pickup':
			let pickupPlayer = state.Players.find((p) => p.Id === action.playerId)
			if (pickupPlayer == null) {
				return state
			}
			let pickupPlayerIndex = state.Players.indexOf(pickupPlayer)
			state.Players[pickupPlayerIndex].HandCards.push({ Id: pickupPlayer.TopKittyCardId } as ICard)
			state.Players[pickupPlayerIndex].TopKittyCardId = -1
			return state

		case 'Play':
			let player = state.Players.find((p: IPlayer) => p.Id === action.playerId)
			if (player == null) {
				return state
			}
			let playerIndex = state.Players.indexOf(player)
			state.Players[playerIndex].HandCards = player.HandCards.filter((c) => c.Id != action.topCard.Id)
			state.CenterPiles[action.centerPileIndex].Cards.push(action.topCard)
			return state

		case 'Replace':
			return action.gameState

		default:
			return state
	}
}
