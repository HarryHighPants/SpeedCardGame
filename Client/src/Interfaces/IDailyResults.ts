export interface IDailyResults {
    completedToday: boolean
    botName: string
    playerWon: boolean
    lostBy: number
    dailyWinStreak: number
    maxDailyWinStreak: number
    dailyWins: number
    dailyLosses: number
}
