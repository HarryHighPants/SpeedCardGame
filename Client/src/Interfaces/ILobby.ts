export interface ILobby {
    connections: IPlayerConnection[]
    isBotGame: boolean
    gameStarted: boolean
}

export interface IPlayerConnection {
    playerId: string
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

export enum RankColour {
    '#b8b8b8',
    '#ff8d63',
    '#ffd26f',
    '#6fd9ff',
    '#ff5050',
    '#ea75ff',
}

export enum BotDifficulty {
    'Easy',
    'Medium',
    'Hard',
    'Impossible',
    'Daily',
}

export type GameType = 'bot' | 'multiplayer'
