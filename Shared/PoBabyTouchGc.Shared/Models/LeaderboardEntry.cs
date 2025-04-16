using System;

namespace PoBabyTouchGc.Shared.Models;

/// <summary>
/// Represents an entry in the leaderboard
/// </summary>
public class LeaderboardEntry
{
    /// <summary>
    /// The rank of the entry (1-10)
    /// </summary>
    public int Rank { get; set; }
    
    /// <summary>
    /// The player's 3-letter initials
    /// </summary>
    public string Initials { get; set; } = string.Empty;
    
    /// <summary>
    /// The player's score
    /// </summary>
    public int Score { get; set; }
    
    /// <summary>
    /// The date the score was achieved
    /// </summary>
    public DateTime Date { get; set; }
}