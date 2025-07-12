using Azure;
using Azure.Data.Tables;

namespace PoBabyTouchGc.Server.Models
{
    /// <summary>
    /// High score entity for Azure Table Storage
    /// Table name: PoBabyTouchGcHighScores
    /// PartitionKey: "HighScores" (allows for future partitioning by difficulty level, game mode, etc.)
    /// RowKey: Timestamp-based unique identifier for ordering
    /// </summary>
    public class HighScore : ITableEntity
    {
        public string PartitionKey { get; set; } = "HighScores";
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        /// <summary>
        /// Player's 3-letter initials (uppercase)
        /// </summary>
        public string PlayerInitials { get; set; } = string.Empty;

        /// <summary>
        /// Player's score
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// When the score was achieved
        /// </summary>
        public DateTime AchievedAt { get; set; }

        /// <summary>
        /// Game mode or difficulty level (optional for future expansion)
        /// </summary>
        public string GameMode { get; set; } = "Default";

        public HighScore()
        {
            // Generate RowKey based on reverse timestamp for descending order
            var ticks = DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks;
            RowKey = ticks.ToString("D19");
            AchievedAt = DateTime.UtcNow;
        }

        public HighScore(string playerInitials, int score, string gameMode = "Default") : this()
        {
            PlayerInitials = playerInitials.ToUpper();
            Score = score;
            GameMode = gameMode;
        }
    }
}