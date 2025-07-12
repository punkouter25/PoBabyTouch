# High Score System Documentation

## Overview
The PoBabyTouchGc application now includes a comprehensive high score system that allows players to:
- Save their high scores with 3-letter initials
- View leaderboards with top scores
- Check if their score qualifies as a high score
- See their ranking compared to other players

## Architecture

### Backend Components

#### 1. HighScore Model (`Models/HighScore.cs`)
- **Purpose**: Represents a high score entry in Azure Table Storage
- **Key Properties**:
  - `PlayerInitials`: 3-letter player initials (uppercase)
  - `Score`: Player's score
  - `AchievedAt`: When the score was achieved
  - `GameMode`: Game difficulty/mode (Default, Easy, Hard, Expert)
- **Table Storage**: Uses `PoBabyTouchGcHighScores` table with partition key "HighScores"

#### 2. HighScoreService (`Services/HighScoreService.cs`)
- **Purpose**: Business logic for managing high scores
- **Key Methods**:
  - `SaveHighScoreAsync()`: Saves a new high score
  - `GetTopScoresAsync()`: Retrieves top scores for a game mode
  - `IsHighScoreAsync()`: Checks if a score qualifies as a high score
  - `GetPlayerRankAsync()`: Gets player's rank for a given score

#### 3. HighScoresController (`Controllers/HighScoresController.cs`)
- **Purpose**: API endpoints for high score operations
- **Endpoints**:
  - `GET /api/highscores`: Get top scores
  - `POST /api/highscores`: Save a new high score
  - `GET /api/highscores/check/{score}`: Check if score is a high score
  - `GET /api/highscores/rank/{score}`: Get player rank for score

### Frontend Components

#### 1. HighScoreEntry Component (`Components/HighScoreEntry.razor`)
- **Purpose**: Modal for entering player initials after achieving a high score
- **Features**:
  - 3-letter initial input with auto-focus progression
  - Real-time validation
  - Shows player's rank
  - Save/Skip options

#### 2. HighScoreDisplay Component (`Components/HighScoreDisplay.razor`)
- **Purpose**: Displays the high score leaderboard
- **Features**:
  - Top 10 scores by default (configurable)
  - Medal icons for top 3 positions
  - Auto-refresh capability
  - Game mode filtering

#### 3. HighScores Page (`Pages/HighScores.razor`)
- **Purpose**: Dedicated page for viewing high scores
- **Features**:
  - Game mode selector
  - Full leaderboard display
  - Navigation back to game

## Usage Examples

### Basic Integration in Game Logic

```csharp
// After game ends, check if it's a high score
private async Task EndGame(int finalScore)
{
    var response = await Http.GetAsync($"api/highscores/check/{finalScore}?gameMode=Default");
    if (response.IsSuccessStatusCode)
    {
        var isHighScore = await response.Content.ReadFromJsonAsync<bool>();
        if (isHighScore)
        {
            ShowHighScoreEntry = true;
            StateHasChanged();
        }
    }
}
```

### Saving High Scores

```csharp
// Save a high score
var request = new
{
    PlayerInitials = "ABC",
    Score = 1500,
    GameMode = "Default"
};

var response = await Http.PostAsJsonAsync("api/highscores", request);
```

### Displaying High Scores

```razor
<!-- In any Blazor component -->
<HighScoreDisplay GameMode="Default" 
                 MaxScores="10" 
                 ShowRefreshButton="true" 
                 AutoRefresh="true" />
```

## API Reference

### POST /api/highscores
Save a new high score.

**Request Body:**
```json
{
  "playerInitials": "ABC",
  "score": 1500,
  "gameMode": "Default"
}
```

**Response:**
- `200 OK`: High score saved successfully
- `400 Bad Request`: Invalid initials or negative score
- `500 Internal Server Error`: Save failed

### GET /api/highscores
Get top high scores.

**Query Parameters:**
- `count` (optional): Number of scores to return (default: 10)
- `gameMode` (optional): Game mode filter (default: "Default")

**Response:**
```json
[
  {
    "playerInitials": "ABC",
    "score": 2500,
    "achievedAt": "2024-01-15T10:30:00Z",
    "gameMode": "Default"
  }
]
```

### GET /api/highscores/check/{score}
Check if a score qualifies as a high score.

**Parameters:**
- `score`: The score to check
- `gameMode` (query): Game mode (default: "Default")

**Response:**
```json
true
```

### GET /api/highscores/rank/{score}
Get the rank for a given score.

**Parameters:**
- `score`: The score to rank
- `gameMode` (query): Game mode (default: "Default")

**Response:**
```json
3
```

## Database Schema

### Azure Table Storage
**Table Name:** `PoBabyTouchGcHighScores`

| Field | Type | Description |
|-------|------|-------------|
| PartitionKey | String | Always "HighScores" |
| RowKey | String | Timestamp-based unique identifier |
| PlayerInitials | String | 3-letter player initials (uppercase) |
| Score | Int32 | Player's score |
| AchievedAt | DateTime | When score was achieved |
| GameMode | String | Game difficulty/mode |

## Testing

### Unit Tests
- `HighScoreServiceTests.cs`: Tests business logic and validation
- Covers invalid inputs, error handling, and data transformations

### Integration Tests
- `HighScoreIntegrationTests.cs`: Tests complete API flow
- Covers all endpoints and Azure Table Storage integration
- Tests multiple game modes and sorting

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test PoBabyTouchGc.UnitTests
dotnet test PoBabyTouchGc.IntegrationTests
```

## Configuration

### Local Development (Azurite)
The system uses Azurite for local development. Ensure Azurite is running:

```bash
# Start Azurite
azurite --silent --location ./AzuriteData --debug ./AzuriteData/debug.log
```

### Production (Azure)
Update connection strings in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "AzureTableStorage": "DefaultEndpointsProtocol=https;AccountName=<account>;AccountKey=<key>;EndpointSuffix=core.windows.net"
  }
}
```

## Security Considerations

1. **Input Validation**: All inputs are validated server-side
2. **Rate Limiting**: Consider implementing rate limiting for high score submissions
3. **Data Sanitization**: Player initials are converted to uppercase and validated
4. **Error Handling**: Comprehensive error handling prevents information leakage

## Performance Considerations

1. **Caching**: Consider implementing caching for frequently accessed leaderboards
2. **Pagination**: Large leaderboards are paginated to prevent performance issues
3. **Indexing**: Azure Table Storage automatically indexes PartitionKey and RowKey
4. **Batch Operations**: Multiple operations can be batched for better performance

## Future Enhancements

1. **Player Profiles**: Extend to support full player profiles
2. **Achievements**: Add achievement system based on scores
3. **Leaderboard Seasons**: Implement time-based leaderboard resets
4. **Social Features**: Add sharing and comparison features
5. **Analytics**: Track score trends and player engagement

## Troubleshooting

### Common Issues

1. **Azurite Not Running**: Ensure Azurite is started before running the application
2. **Invalid Initials**: Check that initials are exactly 3 characters
3. **Connection Errors**: Verify Azure Storage connection string
4. **Missing Scores**: Check that the correct game mode is being used

### Debugging

Enable debug logging to see detailed high score operations:

```json
{
  "Logging": {
    "LogLevel": {
      "PoBabyTouchGc.Server.Services.HighScoreService": "Debug"
    }
  }
}
```

## Conclusion

The high score system provides a complete solution for tracking and displaying player achievements. It's built with scalability, testability, and user experience in mind, following the established patterns and conventions of the PoBabyTouchGc application.