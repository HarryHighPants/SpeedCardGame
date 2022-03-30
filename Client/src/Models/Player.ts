import { Card } from './Card'

export interface Player {
    Id: number
    Name: string
    HandCards: Card[]
    TopKittyCardId: number
    KittyCardsCount: number
    RequestingTopUp: boolean
}
