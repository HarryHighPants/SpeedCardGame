import { Card } from './Card'
import { Player } from './Player'

export interface GameState {
    Players: Player[]
    CenterPiles: Card[]
    lastMove: string
}
