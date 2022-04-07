export interface ICard {
	Id: number
	Suit: Suit | undefined
	CardValue: CardValue | undefined
}

export interface IPos {
	X: number
	Y: number
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
	Hand,
	Kitty,
	Center,
}

export interface IRenderableCard extends ICard {
	pos: IPos
	zIndex: number
	ref: React.RefObject<HTMLDivElement>
	ourCard: boolean
	location: CardLocationType
}

export interface IMovedCardPos {
	cardId: number
	pos: IPos
}
