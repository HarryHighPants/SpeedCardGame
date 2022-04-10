import { ICard } from './ICard'
import { IPlayer } from './IPlayer'

export interface IGameState {
    Players: IPlayer[]
    CenterPiles: CenterPile[]
    LastMove: string
	WinnerId?: number
}

export interface CenterPile {
	Cards: ICard[]
}
