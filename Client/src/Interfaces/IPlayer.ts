import { ICard } from './ICard'

export interface IPlayer {
    Id: number
    Name: string
    HandCards: ICard[]
    TopKittyCardId: number
    KittyCardsCount: number
    RequestingTopUp: boolean
}
