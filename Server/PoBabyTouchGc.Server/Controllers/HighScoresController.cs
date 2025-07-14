using Microsoft.AspNetCore.Mvc;
using PoBabyTouchGc.Shared.Models;
using PoBabyTouchGc.Server.Services;

namespace PoBabyTouchGc.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HighScoresController : ControllerBase
    {
        private readonly IHighScoreService _highScoreService;
        private readonly ILogger<HighScoresController> _logger;

        public HighScoresController(IHighScoreService highScoreService, ILogger<HighScoresController> logger)
        {
            _highScoreService = highScoreService;
            _logger = logger;
        }

        /// <summary>
        /// Get top high scores
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<HighScore>>> GetTopScores(
            [FromQuery] int count = 10, 
            [FromQuery] string gameMode = "Default")
        {
            try
            {
                _logger.LogDebug("Getting top {Count} scores for {GameMode} mode", count, gameMode);
                
                var scores = await _highScoreService.GetTopScoresAsync(count, gameMode);
                return Ok(scores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get top scores");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Save a new high score
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> SaveHighScore([FromBody] SaveHighScoreRequest request)
        {
            try
            {
                _logger.LogDebug("Saving high score: {PlayerInitials} - {Score} points", 
                    request.PlayerInitials, request.Score);

                if (string.IsNullOrWhiteSpace(request.PlayerInitials) || request.PlayerInitials.Length != 3)
                {
                    return BadRequest("Player initials must be exactly 3 characters");
                }

                if (request.Score < 0)
                {
                    return BadRequest("Score cannot be negative");
                }

                var success = await _highScoreService.SaveHighScoreAsync(
                    request.PlayerInitials, 
                    request.Score, 
                    request.GameMode ?? "Default");

                if (success)
                {
                    _logger.LogInformation("High score saved successfully: {PlayerInitials} - {Score} points", 
                        request.PlayerInitials, request.Score);
                    return Ok(new { message = "High score saved successfully" });
                }
                else
                {
                    _logger.LogWarning("Failed to save high score: {PlayerInitials} - {Score} points", 
                        request.PlayerInitials, request.Score);
                    return StatusCode(500, "Failed to save high score");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving high score");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Check if a score qualifies as a high score
        /// </summary>
        [HttpGet("check/{score}")]
        public async Task<ActionResult<bool>> IsHighScore(int score, [FromQuery] string gameMode = "Default")
        {
            try
            {
                _logger.LogDebug("Checking if score {Score} is a high score", score);
                
                var isHighScore = await _highScoreService.IsHighScoreAsync(score, gameMode);
                return Ok(isHighScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if score is high score");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get player rank for a given score
        /// </summary>
        [HttpGet("rank/{score}")]
        public async Task<ActionResult<int>> GetPlayerRank(int score, [FromQuery] string gameMode = "Default")
        {
            try
            {
                _logger.LogDebug("Getting rank for score {Score}", score);
                
                var rank = await _highScoreService.GetPlayerRankAsync(score, gameMode);
                return Ok(rank);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get player rank");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Test endpoint for diagnostics - checks if the service is working
        /// </summary>
        [HttpGet("test")]
        public async Task<ActionResult> TestConnection()
        {
            try
            {
                _logger.LogDebug("Testing high score service connection");
                
                // Try to get top scores as a simple test
                var scores = await _highScoreService.GetTopScoresAsync(1, "Default");
                return Ok(new { status = "connected", message = "High score service is working", scoresCount = scores.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "High score service test failed");
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }

}
