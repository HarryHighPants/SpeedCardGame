import { ICard } from './ICard'
import { IPlayer } from './IPlayer'

export interface IGameState {
    players: IPlayer[]
    centerPiles: CenterPile[] | {}[]
    lastMove: string
    winnerId?: string
    mustTopUp: boolean
}

export interface CenterPile {
    cards: ICard[]
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
            let pickupPlayer = state.players.find((p) => p.id === action.playerId)
            if (pickupPlayer == null) {
                return state
            }
            let pickupPlayerIndex = state.players.indexOf(pickupPlayer)
            state.players[pickupPlayerIndex].handCards.push({ id: pickupPlayer.topKittyCardId } as ICard)
            state.players[pickupPlayerIndex].topKittyCardId = -1
            return { ...state }

        case 'Play':
            let player = state.players.find((p: IPlayer) => p.id === action.playerId)
            if (player == null) {
                return state
            }
            let playerIndex = state.players.indexOf(player)
            state.players[playerIndex].handCards = player.handCards.filter((c) => c.id != action.topCard.id)
            state.centerPiles[action.centerPileIndex].cards.push(action.topCard)
            return { ...state }

        case 'Replace':
            return { ...action.gameState }

        default:
            return state
    }
}
