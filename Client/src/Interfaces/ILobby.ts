export interface ILobby {
	connections: IPlayerConnection[]
	isBotGame: boolean
	gameStarted: boolean
}

export interface IPlayerConnection {
	connectionId: string
	name: string
	rank: Rank
}

export enum Rank {
	'Eager Starter',
	'Dedicated Rival',
	'Certified Racer',
	'Boss Athlete',
	'Ace Champion',
	'Speed Demon',
}

export type GameType = 'bot' | 'multiplayer'
