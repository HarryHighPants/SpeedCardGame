import {Rank} from "./ILobby";

export interface ILeaderboardResults
{
    players: ILeaderboardPlayer[]
    TotalPlayers: number
    botName: string
    botRank: Rank
}

export interface ILeaderboardPlayer
{
    place: number
    name: string
    rank: Rank
    score: number
}