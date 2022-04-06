import { ICard } from './ICard'

export interface IPlayer {
    Id: string
    Name: string
    HandCards: ICard[]
    TopKittyCardId: number
    KittyCardsCount: number
    RequestingTopUp: boolean
	CanRequestTopUp: boolean
	LastMove: string
}
