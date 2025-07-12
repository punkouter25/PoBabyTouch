using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Moq;
using PoBabyTouchGc.Server.Services;
using Xunit;

namespace PoBabyTouchGc.UnitTests.Services
{
    /// <summary>
    /// Unit tests for HighScoreService
    /// Tests business logic and error handling
    /// </summary>
    public class HighScoreServiceTests
    {
        private readonly Mock<TableServiceClient> _mockTableServiceClient;
        private readonly Mock<TableClient> _mockTableClient;
        private readonly Mock<ILogger<HighScoreService>> _mockLogger;
        private readonly HighScoreService _service;

        public HighScoreServiceTests()
        {
            _mockTableServiceClient = new Mock<TableServiceClient>();
            _mockTableClient = new Mock<TableClient>();
            _mockLogger = new Mock<ILogger<HighScoreService>>();

            _mockTableServiceClient.Setup(x => x.GetTableClient(It.IsAny<string>()))
                .Returns(_mockTableClient.Object);

            _service = new HighScoreService(_mockTableServiceClient.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SaveHighScoreAsync_ValidInitials_ReturnsTrue()
        {
            // Arrange
            var playerInitials = "ABC";
            var score = 1500;
            var gameMode = "Default";

            _mockTableClient.Setup(x => x.CreateIfNotExistsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Mock.Of<Azure.Response<TableItem>>()));

            _mockTableClient.Setup(x => x.AddEntityAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Mock.Of<Azure.Response>()));

            // Act
            var result = await _service.SaveHighScoreAsync(playerInitials, score, gameMode);

            // Assert
            Assert.True(result);
            _mockTableClient.Verify(x => x.CreateIfNotExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockTableClient.Verify(x => x.AddEntityAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("AB")]
        [InlineData("ABCD")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task SaveHighScoreAsync_InvalidInitials_ReturnsFalse(string invalidInitials)
        {
            // Arrange
            var score = 1500;
            var gameMode = "Default";

            // Act
            var result = await _service.SaveHighScoreAsync(invalidInitials, score, gameMode);

            // Assert
            Assert.False(result);
            _mockTableClient.Verify(x => x.CreateIfNotExistsAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mockTableClient.Verify(x => x.AddEntityAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SaveHighScoreAsync_ExceptionThrown_ReturnsFalse()
        {
            // Arrange
            var playerInitials = "ABC";
            var score = 1500;
            var gameMode = "Default";

            _mockTableClient.Setup(x => x.CreateIfNotExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Table service error"));

            // Act
            var result = await _service.SaveHighScoreAsync(playerInitials, score, gameMode);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SaveHighScoreAsync_ConvertsInitialsToUppercase()
        {
            // Arrange
            var playerInitials = "abc";
            var score = 1500;
            var gameMode = "Default";

            _mockTableClient.Setup(x => x.CreateIfNotExistsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Mock.Of<Azure.Response<TableItem>>()));

            _mockTableClient.Setup(x => x.AddEntityAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Mock.Of<Azure.Response>()));

            // Act
            var result = await _service.SaveHighScoreAsync(playerInitials, score, gameMode);

            // Assert
            Assert.True(result);
            _mockTableClient.Verify(x => x.AddEntityAsync(
                It.Is<object>(entity => 
                    entity.GetType().GetProperty("PlayerInitials")?.GetValue(entity)?.ToString() == "ABC"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void HighScore_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var playerInitials = "xyz";
            var score = 2500;
            var gameMode = "Expert";

            // Act
            var highScore = new PoBabyTouchGc.Server.Models.HighScore(playerInitials, score, gameMode);

            // Assert
            Assert.Equal("XYZ", highScore.PlayerInitials);
            Assert.Equal(2500, highScore.Score);
            Assert.Equal("Expert", highScore.GameMode);
            Assert.Equal("HighScores", highScore.PartitionKey);
            Assert.NotEmpty(highScore.RowKey);
            Assert.True(highScore.AchievedAt <= DateTime.UtcNow);
            Assert.True(highScore.AchievedAt >= DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public void HighScore_RowKey_IsUniqueAndSortable()
        {
            // Arrange & Act
            var highScore1 = new PoBabyTouchGc.Server.Models.HighScore("ABC", 1000);
            Thread.Sleep(1); // Ensure different timestamps
            var highScore2 = new PoBabyTouchGc.Server.Models.HighScore("DEF", 2000);

            // Assert
            Assert.NotEqual(highScore1.RowKey, highScore2.RowKey);
            Assert.True(string.Compare(highScore1.RowKey, highScore2.RowKey) > 0); // Later scores should have "smaller" RowKeys for descending order
        }

        [Fact]
        public void HighScore_DefaultConstructor_SetsDefaults()
        {
            // Act
            var highScore = new PoBabyTouchGc.Server.Models.HighScore();

            // Assert
            Assert.Equal("HighScores", highScore.PartitionKey);
            Assert.NotEmpty(highScore.RowKey);
            Assert.Equal(string.Empty, highScore.PlayerInitials);
            Assert.Equal(0, highScore.Score);
            Assert.Equal("Default", highScore.GameMode);
            Assert.True(highScore.AchievedAt <= DateTime.UtcNow);
        }
    }
}