using System.ComponentModel.DataAnnotations;

namespace PoBabyTouchGc.Shared.Models;

/// <summary>
/// Represents a score submission from the client
/// </summary>
public class ScoreSubmission
{
    /// <summary>
    /// The player's 3-letter initials
    /// </summary>
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Initials must be exactly 3 characters")]
    public string Initials { get; set; } = string.Empty;
    
    /// <summary>
    /// The player's score
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Score must be greater than 0")]
    public int Score { get; set; }
}