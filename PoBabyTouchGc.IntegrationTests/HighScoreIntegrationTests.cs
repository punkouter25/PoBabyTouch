using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PoBabyTouchGc.Server.Services;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace PoBabyTouchGc.IntegrationTests
{
    /// <summary>
    /// Integration tests for high score functionality
    /// Tests the complete flow from API to Azure Table Storage
    /// </summary>
    public class HighScoreIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public HighScoreIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task SaveHighScore_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new
            {
                PlayerInitials = "ABC",
                Score = 1500,
                GameMode = "Default"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/highscores", request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.True(result.TryGetProperty("message", out var message));
            Assert.Equal("High score saved successfully", message.GetString());
        }

        [Fact]
        public async Task SaveHighScore_InvalidInitials_ReturnsBadRequest()
        {
            // Arrange
            var request = new
            {
                PlayerInitials = "AB", // Only 2 characters
                Score = 1500,
                GameMode = "Default"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/highscores", request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SaveHighScore_NegativeScore_ReturnsBadRequest()
        {
            // Arrange
            var request = new
            {
                PlayerInitials = "ABC",
                Score = -100,
                GameMode = "Default"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/highscores", request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTopScores_DefaultRequest_ReturnsScores()
        {
            // Arrange - First save a high score
            var saveRequest = new
            {
                PlayerInitials = "XYZ",
                Score = 2000,
                GameMode = "Default"
            };
            await _client.PostAsJsonAsync("/api/highscores", saveRequest);

            // Act
            var response = await _client.GetAsync("/api/highscores");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var scores = JsonSerializer.Deserialize<JsonElement[]>(content);
            Assert.NotNull(scores);
            Assert.True(scores.Length > 0);
            
            // Check that scores are sorted by score descending
            if (scores.Length > 1)
            {
                for (int i = 0; i < scores.Length - 1; i++)
                {
                    var currentScore = scores[i].GetProperty("score").GetInt32();
                    var nextScore = scores[i + 1].GetProperty("score").GetInt32();
                    Assert.True(currentScore >= nextScore);
                }
            }
        }

        [Fact]
        public async Task GetTopScores_WithCount_ReturnsCorrectNumber()
        {
            // Arrange - Save multiple high scores
            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                var request = new
                {
                    PlayerInitials = $"T{i:D2}",
                    Score = 1000 + (i * 100),
                    GameMode = "Default"
                };
                tasks.Add(_client.PostAsJsonAsync("/api/highscores", request));
            }
            await Task.WhenAll(tasks);

            // Act
            var response = await _client.GetAsync("/api/highscores?count=3");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var scores = JsonSerializer.Deserialize<JsonElement[]>(content);
            Assert.NotNull(scores);
            Assert.True(scores.Length <= 3);
        }

        [Fact]
        public async Task CheckHighScore_HighScore_ReturnsTrue()
        {
            // Arrange - Save a lower score first
            var saveRequest = new
            {
                PlayerInitials = "LOW",
                Score = 500,
                GameMode = "Test"
            };
            await _client.PostAsJsonAsync("/api/highscores", saveRequest);

            // Act - Check if 1000 is a high score
            var response = await _client.GetAsync("/api/highscores/check/1000?gameMode=Test");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var isHighScore = JsonSerializer.Deserialize<bool>(content);
            Assert.True(isHighScore);
        }

        [Fact]
        public async Task CheckHighScore_NotHighScore_ReturnsFalse()
        {
            // Arrange - Save multiple high scores
            var tasks = new List<Task>();
            for (int i = 0; i < 12; i++) // More than 10 to fill the leaderboard
            {
                var request = new
                {
                    PlayerInitials = $"H{i:D2}",
                    Score = 2000 + (i * 100),
                    GameMode = "TestFull"
                };
                tasks.Add(_client.PostAsJsonAsync("/api/highscores", request));
            }
            await Task.WhenAll(tasks);

            // Act - Check if 1000 is a high score (it shouldn't be)
            var response = await _client.GetAsync("/api/highscores/check/1000?gameMode=TestFull");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var isHighScore = JsonSerializer.Deserialize<bool>(content);
            Assert.False(isHighScore);
        }

        [Fact]
        public async Task GetPlayerRank_ValidScore_ReturnsCorrectRank()
        {
            // Arrange - Save scores with known values
            var scores = new[] { 3000, 2500, 2000, 1500, 1000 };
            var tasks = new List<Task>();
            
            for (int i = 0; i < scores.Length; i++)
            {
                var request = new
                {
                    PlayerInitials = $"R{i:D2}",
                    Score = scores[i],
                    GameMode = "Rank"
                };
                tasks.Add(_client.PostAsJsonAsync("/api/highscores", request));
            }
            await Task.WhenAll(tasks);

            // Act - Check rank for score 1750 (should be rank 4)
            var response = await _client.GetAsync("/api/highscores/rank/1750?gameMode=Rank");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var rank = JsonSerializer.Deserialize<int>(content);
            Assert.Equal(4, rank);
        }

        [Fact]
        public async Task HighScoreService_CompleteFlow_WorksEndToEnd()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var highScoreService = scope.ServiceProvider.GetRequiredService<IHighScoreService>();
            var gameMode = "EndToEnd";

            // Act & Assert - Save a high score
            var saveResult = await highScoreService.SaveHighScoreAsync("E2E", 1800, gameMode);
            Assert.True(saveResult);

            // Act & Assert - Get top scores
            var topScores = await highScoreService.GetTopScoresAsync(10, gameMode);
            Assert.NotNull(topScores);
            Assert.Contains(topScores, s => s.PlayerInitials == "E2E" && s.Score == 1800);

            // Act & Assert - Check if it's a high score
            var isHighScore = await highScoreService.IsHighScoreAsync(1900, gameMode);
            Assert.True(isHighScore);

            // Act & Assert - Get player rank
            var rank = await highScoreService.GetPlayerRankAsync(1900, gameMode);
            Assert.Equal(1, rank);
        }

        [Theory]
        [InlineData("")]
        [InlineData("AB")]
        [InlineData("ABCD")]
        [InlineData(null)]
        public async Task SaveHighScore_InvalidInitials_ReturnsFalse(string initials)
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var highScoreService = scope.ServiceProvider.GetRequiredService<IHighScoreService>();

            // Act
            var result = await highScoreService.SaveHighScoreAsync(initials, 1000, "Test");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HighScoreService_MultipleGameModes_KeepsSeparateLeaderboards()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var highScoreService = scope.ServiceProvider.GetRequiredService<IHighScoreService>();

            // Act - Save scores in different game modes
            await highScoreService.SaveHighScoreAsync("EZ1", 1000, "Easy");
            await highScoreService.SaveHighScoreAsync("HD1", 2000, "Hard");
            await highScoreService.SaveHighScoreAsync("EZ2", 1200, "Easy");

            // Assert - Check that each mode has its own leaderboard
            var easyScores = await highScoreService.GetTopScoresAsync(10, "Easy");
            var hardScores = await highScoreService.GetTopScoresAsync(10, "Hard");

            Assert.Equal(2, easyScores.Count);
            Assert.Single(hardScores);
            Assert.All(easyScores, s => Assert.Equal("Easy", s.GameMode));
            Assert.All(hardScores, s => Assert.Equal("Hard", s.GameMode));
        }
    }
}