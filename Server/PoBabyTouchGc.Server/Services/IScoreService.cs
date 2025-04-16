using Azure.Data.Tables;
using PoBabyTouchGc.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoBabyTouchGc.Server.Services;

/// <summary>
/// Interface for the Score service that handles leaderboard operations
/// </summary>
public interface IScoreService
{
    /// <summary>
    /// Gets the top 10 scores from the leaderboard
    /// </summary>
    /// <returns>A list of the top 10 scores</returns>
    Task<List<LeaderboardEntry>> GetTopScoresAsync();

    /// <summary>
    /// Submits a new score to the leaderboard
    /// </summary>
    /// <param name="scoreSubmission">The score submission data</param>
    /// <returns>True if the score was submitted successfully, false otherwise</returns>
    Task<bool> SubmitScoreAsync(ScoreSubmission scoreSubmission);
}