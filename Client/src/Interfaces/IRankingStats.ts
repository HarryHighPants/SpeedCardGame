import { Rank } from './ILobby'

export interface IRankingStats {
    // oldRank: Rank
    // newRank: Rank
    previousElo: number
    newElo: number
}
