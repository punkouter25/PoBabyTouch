using Azure;
using Azure.Data.Tables;
using PoBabyTouchGc.Shared.Models;

namespace PoBabyTouchGc.Server.Services
{
    /// <summary>
    /// Service for managing high scores in Azure Table Storage
    /// Uses Repository pattern for data access abstraction
    /// </summary>
    public interface IHighScoreService
    {
        Task<bool> SaveHighScoreAsync(string playerInitials, int score, string gameMode = "Default");
        Task<List<HighScore>> GetTopScoresAsync(int count = 10, string gameMode = "Default");
        Task<bool> IsHighScoreAsync(int score, string gameMode = "Default");
        Task<int> GetPlayerRankAsync(int score, string gameMode = "Default");
    }

    public class HighScoreService : IHighScoreService
    {
        private readonly TableClient _tableClient;
        private readonly ILogger<HighScoreService> _logger;
        private const string TableName = "PoBabyTouchGcHighScores";

        public HighScoreService(TableServiceClient tableServiceClient, ILogger<HighScoreService> logger)
        {
            _tableClient = tableServiceClient.GetTableClient(TableName);
            _logger = logger;
        }

        public async Task<bool> SaveHighScoreAsync(string playerInitials, int score, string gameMode = "Default")
        {
            try
            {
                _logger.LogDebug("Saving high score: {PlayerInitials} - {Score} points in {GameMode} mode", 
                    playerInitials, score, gameMode);

                // Validate initials (must be exactly 3 characters)
                if (string.IsNullOrWhiteSpace(playerInitials) || playerInitials.Length != 3)
                {
                    _logger.LogWarning("Invalid player initials: {PlayerInitials}. Must be exactly 3 characters.", playerInitials);
                    return false;
                }

                // Ensure table exists
                await _tableClient.CreateIfNotExistsAsync();

                var highScore = new HighScore(gameMode, playerInitials, score);
                await _tableClient.AddEntityAsync(highScore);

                _logger.LogInformation("High score saved successfully: {PlayerInitials} - {Score} points", 
                    playerInitials, score);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save high score for {PlayerInitials} - {Score} points", 
                    playerInitials, score);
                return false;
            }
        }

        public async Task<List<HighScore>> GetTopScoresAsync(int count = 10, string gameMode = "Default")
        {
            try
            {
                _logger.LogDebug("Retrieving top {Count} scores for {GameMode} mode", count, gameMode);

                // Ensure table exists
                await _tableClient.CreateIfNotExistsAsync();

                var query = _tableClient.QueryAsync<HighScore>(
                    filter: $"PartitionKey eq '{gameMode}'",
                    maxPerPage: count
                );

                var highScores = new List<HighScore>();
                await foreach (var score in query)
                {
                    highScores.Add(score);
                    if (highScores.Count >= count) break;
                }

                // Sort by score descending (in case Azure ordering isn't perfect)
                var sortedScores = highScores.OrderByDescending(s => s.Score).Take(count).ToList();

                _logger.LogInformation("Retrieved {Count} high scores for {GameMode} mode", 
                    sortedScores.Count, gameMode);
                return sortedScores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve high scores for {GameMode} mode", gameMode);
                return new List<HighScore>();
            }
        }

        public async Task<bool> IsHighScoreAsync(int score, string gameMode = "Default")
        {
            try
            {
                var topScores = await GetTopScoresAsync(10, gameMode);
                
                // If we have less than 10 scores, any score is a high score
                if (topScores.Count < 10)
                {
                    _logger.LogDebug("Score {Score} qualifies as high score (less than 10 scores exist)", score);
                    return true;
                }

                // Check if score is higher than the lowest top score
                var lowestTopScore = topScores.Min(s => s.Score);
                var isHighScore = score > lowestTopScore;

                _logger.LogDebug("Score {Score} is {IsHighScore} a high score (lowest top score: {LowestTopScore})", 
                    score, isHighScore ? "" : "not", lowestTopScore);
                return isHighScore;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if score {Score} is a high score", score);
                return false;
            }
        }

        public async Task<int> GetPlayerRankAsync(int score, string gameMode = "Default")
        {
            try
            {
                var topScores = await GetTopScoresAsync(100, gameMode); // Get more scores for accurate ranking
                var rank = topScores.Count(s => s.Score > score) + 1;

                _logger.LogDebug("Score {Score} ranks #{Rank} out of {TotalScores} scores", 
                    score, rank, topScores.Count);
                return rank;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get player rank for score {Score}", score);
                return -1;
            }
        }
    }
}