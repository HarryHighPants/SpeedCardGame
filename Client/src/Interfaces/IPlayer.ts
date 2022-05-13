import { ICard } from './ICard'

export interface IPlayer {
    idHash: string
    name: string
    handCards: ICard[]
    topKittyCardId?: number
    kittyCardsCount: number
    requestingTopUp: boolean
    canRequestTopUp: boolean
    lastMove: string
}
