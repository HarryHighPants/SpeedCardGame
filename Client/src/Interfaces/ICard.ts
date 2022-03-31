export interface ICard {
    Id: number
    Suit: Suit
    CardValue: CardValue
}

export interface IPos {
    x: number
    y: number
}

export enum CardValue {
    Two = 0,
    Three = 1,
    Four = 2,
    Five = 3,
    Six = 4,
    Seven = 5,
    Eight = 6,
    Nine = 7,
    Ten = 8,
    Jack = 9,
    Queen = 10,
    King = 11,
    Ace = 12,
}

export enum Suit {
    Hearts,
    Diamonds,
    Clubs,
    Spades,
}

export interface IRenderableCard extends ICard {
    pos: IPos | undefined | null
    draggable: boolean
    droppableTarget: boolean
}
