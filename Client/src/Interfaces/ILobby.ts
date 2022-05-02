export interface ILobby {
	Connections: IPlayerConnection[]
	IsBotGame: boolean
	GameStarted: boolean
}

export interface IPlayerConnection {
	ConnectionId: string
	Name: string
	Rank: Rank
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
