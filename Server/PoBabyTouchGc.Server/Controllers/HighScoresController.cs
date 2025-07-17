using Microsoft.AspNetCore.Mvc;
using PoBabyTouchGc.Shared.Models;
using PoBabyTouchGc.Server.Services;
using HighScore = PoBabyTouchGc.Shared.Models.HighScore; // Use the shared model for API responses

namespace PoBabyTouchGc.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HighScoresController : ControllerBase
    {
        private readonly IHighScoreService _highScoreService;
        private readonly IHighScoreValidationService _validationService;
        private readonly ILogger<HighScoresController> _logger;

        public HighScoresController(
            IHighScoreService highScoreService,
            IHighScoreValidationService validationService,
            ILogger<HighScoresController> logger)
        {
            _highScoreService = highScoreService;
            _validationService = validationService;
            _logger = logger;
        }

        /// <summary>
        /// Get top high scores
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<HighScore>>>> GetTopScores(
            [FromQuery] int count = 10,
            [FromQuery] string gameMode = "Default")
        {
            try
            {
                _logger.LogDebug("Getting top {Count} scores for {GameMode} mode", count, gameMode);

                var scores = await _highScoreService.GetTopScoresAsync(count, gameMode);
                return Ok(ApiResponse<List<HighScore>>.SuccessResult(scores));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get top scores");
                return StatusCode(500, ApiResponse<List<HighScore>>.ErrorResult("Failed to retrieve top scores"));
            }
        }

        /// <summary>
        /// Save a new high score
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> SaveHighScore([FromBody] SaveHighScoreRequest request)
        {
            try
            {
                _logger.LogDebug("Saving high score: {PlayerInitials} - {Score} points",
                    request.PlayerInitials, request.Score);

                // Use validation service instead of inline validation
                var validationResult = _validationService.ValidateHighScore(request);

                if (!validationResult.IsValid)
                {
                    var validationResponse = ApiResponse<object>.ErrorResult(validationResult.ErrorMessage ?? "Validation Error");
                    return BadRequest(validationResponse);
                }

                var success = await _highScoreService.SaveHighScoreAsync(
                    request.PlayerInitials,
                    request.Score,
                    request.GameMode ?? "Default");

                if (success)
                {
                    _logger.LogInformation("High score saved successfully: {PlayerInitials} - {Score} points",
                        request.PlayerInitials, request.Score);
                    var response = ApiResponse<object>.SuccessResult(new { message = "High score saved successfully" }, "High score saved successfully");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Failed to save high score: {PlayerInitials} - {Score} points",
                        request.PlayerInitials, request.Score);
                    var response = ApiResponse<object>.ErrorResult("Failed to save high score");
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving high score");
                var response = ApiResponse<object>.ErrorResult("Internal server error");
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Check if a score qualifies as a high score
        /// </summary>
        [HttpGet("check/{score}")]
        public async Task<ActionResult<ApiResponse<bool>>> IsHighScore(int score, [FromQuery] string gameMode = "Default")
        {
            try
            {
                _logger.LogDebug("Checking if score {Score} is a high score", score);

                var isHighScore = await _highScoreService.IsHighScoreAsync(score, gameMode);
                return Ok(ApiResponse<bool>.SuccessResult(isHighScore));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if score is high score");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Failed to check if score is high score"));
            }
        }

        /// <summary>
        /// Get player rank for a given score
        /// </summary>
        [HttpGet("rank/{score}")]
        public async Task<ActionResult<ApiResponse<int>>> GetPlayerRank(int score, [FromQuery] string gameMode = "Default")
        {
            try
            {
                _logger.LogDebug("Getting rank for score {Score}", score);

                var rank = await _highScoreService.GetPlayerRankAsync(score, gameMode);
                return Ok(ApiResponse<int>.SuccessResult(rank));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get player rank");
                return StatusCode(500, ApiResponse<int>.ErrorResult("Failed to get player rank"));
            }
        }

        /// <summary>
        /// Test endpoint for diagnostics - checks if the service is working
        /// </summary>
        [HttpGet("test")]
        public async Task<ActionResult<ApiResponse<object>>> TestConnection()
        {
            try
            {
                _logger.LogDebug("Testing high score service connection");

                // Try to get top scores as a simple test
                var scores = await _highScoreService.GetTopScoresAsync(1, "Default");
                var testData = new { status = "connected", message = "High score service is working", scoresCount = scores.Count };
                var response = ApiResponse<object>.SuccessResult(testData, "Service test completed successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "High score service test failed");
                var response = ApiResponse<object>.ErrorResult("High score service test failed");
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Diagnostic endpoint for troubleshooting Azure Table Storage connection
        /// </summary>
        [HttpGet("diagnostics")]
        public async Task<ActionResult<ApiResponse<object>>> GetDiagnostics()
        {
            try
            {
                _logger.LogDebug("Running diagnostics for high score service");

                var diagnosticInfo = new
                {
                    status = "running",
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    connectionTest = "attempting...",
                    tableTest = "pending...",
                    serviceTest = "pending..."
                };

                // Test connection by attempting to get service properties
                try
                {
                    await _highScoreService.GetTopScoresAsync(1, "Default");
                    diagnosticInfo = diagnosticInfo with { 
                        connectionTest = "success",
                        tableTest = "success",
                        serviceTest = "success"
                    };
                }
                catch (Exception serviceEx)
                {
                    _logger.LogError(serviceEx, "High score service diagnostic test failed");
                    diagnosticInfo = diagnosticInfo with { 
                        connectionTest = "failed",
                        tableTest = "failed",
                        serviceTest = $"failed: {serviceEx.Message}"
                    };
                }

                var response = ApiResponse<object>.SuccessResult(diagnosticInfo, "Diagnostics completed");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Diagnostics endpoint failed");
                var response = ApiResponse<object>.ErrorResult($"Diagnostics failed: {ex.Message}");
                return StatusCode(500, response);
            }
        }
    }

}
