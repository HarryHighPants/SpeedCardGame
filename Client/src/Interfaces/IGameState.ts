import { ICard } from './ICard'
import { IPlayer } from './IPlayer'

export interface IGameState {
    Players: IPlayer[]
    CenterPiles: ICard[]
    lastMove: string
}
