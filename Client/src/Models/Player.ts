import { Card } from './Card'

export interface Player {
    Id: number
    Name: string
    HandCards: Card[]
    KittyCardsCount: number
    RequestingTopUp: boolean
}
