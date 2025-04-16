using Azure.Data.Tables;
using PoBabyTouchGc.Shared.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoBabyTouchGc.Server.Services;

/// <summary>
/// Service for handling score and leaderboard operations using Azure Table Storage
/// </summary>
public class ScoreService : IScoreService
{
    private readonly TableClient _tableClient;
    private const string PartitionKey = "Scores";

    /// <summary>
    /// Constructor for ScoreService
    /// </summary>
    /// <param name="tableClient">The Azure Table Storage client</param>
    public ScoreService(TableClient tableClient)
    {
        _tableClient = tableClient;
        Log.Information("ScoreService initialized with table client for {TableName}", _tableClient.Name);
    }

    /// <summary>
    /// Gets the top 10 scores from the leaderboard
    /// </summary>
    /// <returns>A list of the top 10 scores</returns>
    public async Task<List<LeaderboardEntry>> GetTopScoresAsync()
    {
        try
        {
            Log.Information("Retrieving top 10 scores from leaderboard");
            
            // Query the table for all scores with the Scores partition key, ordered by score descending
            var query = _tableClient.QueryAsync<ScoreEntity>(
                filter: $"PartitionKey eq '{PartitionKey}'",
                maxPerPage: 10);

            var scores = new List<ScoreEntity>();

            await foreach (var score in query)
            {
                scores.Add(score);
            }

            // Sort the scores by Score descending and take the top 10
            var topScores = scores
                .OrderByDescending(s => s.Score)
                .Take(10)
                .Select(s => new LeaderboardEntry
                {
                    Rank = 0, // Will be set below
                    Initials = s.Initials,
                    Score = s.Score,
                    Date = s.Timestamp?.DateTime ?? DateTime.UtcNow
                })
                .ToList();

            // Set the rank for each score
            for (int i = 0; i < topScores.Count; i++)
            {
                topScores[i].Rank = i + 1;
            }

            Log.Information("Retrieved {Count} scores from leaderboard", topScores.Count);
            return topScores;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving top scores from leaderboard");
            return new List<LeaderboardEntry>();
        }
    }

    /// <summary>
    /// Submits a new score to the leaderboard
    /// </summary>
    /// <param name="scoreSubmission">The score submission data</param>
    /// <returns>True if the score was submitted successfully, false otherwise</returns>
    public async Task<bool> SubmitScoreAsync(ScoreSubmission scoreSubmission)
    {
        try
        {
            if (scoreSubmission == null || string.IsNullOrWhiteSpace(scoreSubmission.Initials) || scoreSubmission.Score <= 0)
            {
                Log.Warning("Invalid score submission: {@ScoreSubmission}", scoreSubmission);
                return false;
            }

            // Ensure initials are exactly 3 characters, uppercase
            var initials = scoreSubmission.Initials.ToUpper().PadRight(3).Substring(0, 3);

            // Create a unique row key based on a combination of score, initials, and timestamp
            var rowKey = $"{scoreSubmission.Score:D10}_{initials}_{DateTime.UtcNow.Ticks}";

            var scoreEntity = new ScoreEntity
            {
                PartitionKey = PartitionKey,
                RowKey = rowKey,
                Initials = initials,
                Score = scoreSubmission.Score
            };

            Log.Information("Submitting score: {@ScoreEntity}", scoreEntity);

            // Add the score to the table
            await _tableClient.AddEntityAsync(scoreEntity);
            
            Log.Information("Successfully submitted score for {Initials} with score {Score}", initials, scoreSubmission.Score);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error submitting score for {Initials} with score {Score}", 
                scoreSubmission?.Initials, scoreSubmission?.Score);
            return false;
        }
    }
}