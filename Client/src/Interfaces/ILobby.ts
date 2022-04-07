export interface ILobby {
	Connections: IPlayerConnection[]
	IsBotGame: boolean
	GameStarted: boolean
}

export interface IPlayerConnection {
	ConnectionId: string
	Name: string
}

export type GameType = 'bot' | 'multiplayer'
