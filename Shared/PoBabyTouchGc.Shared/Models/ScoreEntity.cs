using Azure;
using Azure.Data.Tables;
using System;

namespace PoBabyTouchGc.Shared.Models;

/// <summary>
/// Represents a score entity in Azure Table Storage
/// </summary>
public class ScoreEntity : ITableEntity
{
    /// <summary>
    /// The partition key for the entity
    /// </summary>
    public string PartitionKey { get; set; } = string.Empty;
    
    /// <summary>
    /// The row key for the entity
    /// </summary>
    public string RowKey { get; set; } = string.Empty;
    
    /// <summary>
    /// The player's 3-letter initials
    /// </summary>
    public string Initials { get; set; } = string.Empty;
    
    /// <summary>
    /// The player's score
    /// </summary>
    public int Score { get; set; }
    
    /// <summary>
    /// The timestamp for the entity
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }
    
    /// <summary>
    /// The ETag for the entity
    /// </summary>
    public ETag ETag { get; set; }
}