using Microsoft.AspNetCore.Mvc;
using PoBabyTouchGc.Server.Services;

namespace PoBabyTouchGc.Server.Controllers
{
    /// <summary>
    /// Alias controller for backward compatibility - maps to HighScoresController
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ScoresController : ControllerBase
    {
        private readonly IHighScoreService _highScoreService;
        private readonly ILogger<ScoresController> _logger;

        public ScoresController(IHighScoreService highScoreService, ILogger<ScoresController> logger)
        {
            _highScoreService = highScoreService;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint for diagnostics
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> TestConnection()
        {
            try
            {
                _logger.LogDebug("Testing connection via scores endpoint");
                
                var scores = await _highScoreService.GetTopScoresAsync(1, "Default");
                return Ok(new { status = "connected", message = "Service is working", scoresCount = scores.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed");
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        /// <summary>
        /// Account endpoint for diagnostics (placeholder)
        /// </summary>
        [HttpGet("account")]
        public ActionResult GetAccount()
        {
            return Ok(new { status = "active", account = "PoBabyTouch" });
        }
    }
}
