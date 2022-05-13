export interface ICard {
    id: number
    suit: Suit | undefined
    cardValue: CardValue | undefined
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

export enum CardLocationType {
    Undefined,
    Hand,
    Kitty,
    TopUp,
    Center,
}

export interface IRenderableCard extends ICard {
    pos: IPos
    isCustomPos: boolean
    zIndex: number
    ref: React.RefObject<HTMLDivElement>
    ourCard: boolean
    location: CardLocationType
    pileIndex: number
    animateInHorizontalOffset: number | undefined
    animateInDelay: number
    animateInZIndex: number
    startTransparent: boolean
    horizontalOffset: number
    forceUpdate: () => void
}

export interface IMovedCardPos {
    cardId: number
    pos: IPos | null
}
