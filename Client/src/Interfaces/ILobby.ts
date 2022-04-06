export interface ILobby {
	connections: IPlayerConnection[]
	isBotGame: boolean
	gameStarted: boolean
}

export interface IPlayerConnection {
	connectionId: string
	name: string
}
//
// export enum GameType {
// 	multiplayer="multiplayer",
// 	bot="bot"
// }

export type GameType = 'bot' | 'multiplayer'
