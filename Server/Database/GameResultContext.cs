namespace Server.Models.Database;

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public class GameResultContext : DbContext
{
    public DbSet<GameResultDao?> GameResults { get; set; }
    public DbSet<PlayerDao> Players { get; set; }

    public GameResultContext(DbContextOptions<GameResultContext> options) : base(options) { }
}

[Table("GameResult")]
public class GameResultDao
{
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public PlayerDao Winner { get; set; }
    public PlayerDao Loser { get; set; }
    public int Turns { get; set; }
    public int LostBy { get; set; }
    public bool Daily { get; set; }
    public int DailyIndex { get; set; }
    
    public int EloGained { get; set; }

    public int EloLost { get; set; }
}

[Table("Player")]
public class PlayerDao
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsBot { get; set; }
    public int Elo { get; set; }
    public int DailyWinStreak { get; set; }
    public int MaxDailyWinStreak { get; set; }
    public int DailyWins { get; set; }
    public int Wins { get; set; }
    public int DailyLosses { get; set; }
    public int Losses { get; set; }
}
