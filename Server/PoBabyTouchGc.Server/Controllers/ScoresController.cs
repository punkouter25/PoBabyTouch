using Microsoft.AspNetCore.Mvc;
using PoBabyTouchGc.Server.Services;
using PoBabyTouchGc.Shared.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoBabyTouchGc.Server.Controllers;

/// <summary>
/// Controller for handling score-related API requests
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ScoresController : ControllerBase
{
    private readonly IScoreService _scoreService;

    /// <summary>
    /// Constructor for ScoresController
    /// </summary>
    /// <param name="scoreService">The score service</param>
    public ScoresController(IScoreService scoreService)
    {
        _scoreService = scoreService;
    }

    /// <summary>
    /// Gets the top 10 scores from the leaderboard
    /// </summary>
    /// <returns>The top 10 scores</returns>
    [HttpGet]
    public async Task<ActionResult<List<LeaderboardEntry>>> GetTopScores()
    {
        try
        {
            Log.Information("ScoresController.GetTopScores: API Request: Get top scores");
            var scores = await _scoreService.GetTopScoresAsync();
            return Ok(scores);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ScoresController.GetTopScores: Error getting top scores");
            return StatusCode(500, "An error occurred while retrieving the leaderboard");
        }
    }

    /// <summary>
    /// Submits a new score to the leaderboard
    /// </summary>
    /// <param name="submission">The score submission data</param>
    /// <returns>OK if successful, BadRequest if validation fails, InternalServerError if an error occurs</returns>
    [HttpPost]
    public async Task<IActionResult> SubmitScore([FromBody] ScoreSubmission submission)
    {
        try
        {
            if (!ModelState.IsValid)
            {
            Log.Warning("ScoresController.SubmitScore: Invalid score submission: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            Log.Information("ScoresController.SubmitScore: API Request: Submit score {@ScoreSubmission}", submission);
            var result = await _scoreService.SubmitScoreAsync(submission);
            
            if (result)
            {
                return Ok();
            }
            else
            {
                // Log the failure from the service layer if it wasn't an exception
                Log.Warning("ScoresController.SubmitScore: Score submission failed for submission: {@ScoreSubmission}. Service returned false.", submission);
                return BadRequest("Failed to submit score");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ScoresController.SubmitScore: Error submitting score: {@ScoreSubmission}", submission);
            return StatusCode(500, "An error occurred while submitting your score");
        }
    }
}
