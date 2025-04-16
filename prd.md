PoBabyTouch Game: Comprehensive Project Plan
Application Overview
PoBabyTouch is an interactive web-based game designed primarily for mobile devices in portrait orientation, built using Blazor WebAssembly technology. The core gameplay revolves around colorful circles representing three individuals (Matt, Nick, and Kim) that bounce around the screen with realistic physics-based movement. Players must tap or touch these circles as quickly as possible within a 20-second time limit to earn points, with each successful touch earning one point. Upon contact, circles momentarily disappear with an engaging visual effect before regenerating in a new, non-overlapping position on the screen.

The game features an intuitive scoring system tied to an online leaderboard that tracks the top 10 high scores across all players, requiring players who achieve a top-ranking score to enter their three-letter initials for posterity. As the countdown timer progresses, the difficulty gradually increases, challenging players to improve their reaction time and coordination skills. When a circle representing a specific person is touched, the game will play a randomly selected audio clip from a collection of five unique sounds associated with that person, adding an auditory dimension to the gameplay experience.

PoBabyTouch employs a kid-friendly aesthetic with bright colors and simple interface elements that appeal to younger players while remaining engaging for users of all ages. The application features a clean, minimalist main menu that allows players to start a new game or view the current leaderboard standings. From a technical standpoint, the game utilizes a Blazor WebAssembly frontend for responsive, client-side gameplay coupled with an ASP.NET Core Web API backend that handles persistent data storage via Azure Table Storage services.

The physics engine implemented within the game creates realistic bouncing behaviors, with circles colliding against both screen boundaries and each other based on momentum and direction vectors. This creates an unpredictable and dynamic play environment where each game session feels unique. The combination of time pressure, increasing difficulty, audiovisual feedback, and competitive leaderboard elements creates a compelling loop that encourages repeated play sessions as users attempt to climb the rankings.

Page Descriptions and Functionality
1. Home.razor (Main Menu)
The Home.razor page serves as the application's entry point and main navigation hub. It features a vertically-oriented layout optimized for mobile devices with the following elements:

Header Section: Contains the game's title "PoBabyTouch" in a playful, kid-friendly font at the top of the screen.
Main Menu Buttons: Prominently displayed in the center of the screen with two primary options:
"Play Game": A large, colorful button that transitions the user to the Game.razor page to begin gameplay.
"Leaderboard": A button that navigates the user to the Leader.razor page to view high scores.
Footer Section: Contains minimal information such as version number and copyright information.
Background: Features a subtle, animated background with slowly moving shapes that hint at the game's physics-based nature without being distracting.
Responsive Design: All elements automatically adjust based on screen dimensions while maintaining the portrait orientation.
The Home page's primary responsibility is to provide clear navigation options and establish the game's visual identity and theme. It loads quickly and requires minimal resources, serving as a lightweight launching point for the more resource-intensive gameplay experience.

2. Game.razor (Gameplay Screen)
The Game.razor page is the core interactive component where the actual gameplay occurs. It implements the following features and elements:

Score Display: Positioned at the top-center of the screen, showing the current score (starting at 0) in a clear, easily readable format.
Timer Display: Adjacent to the score display, showing the remaining time in seconds (counting down from 20).
Game Area: The main portion of the screen where the seven bouncing circles appear and interact. This area spans most of the viewport.
Circle Elements: Seven colored circles representing the three individuals (Matt, Nick, Kim) distributed randomly. Each circle is approximately 5% of the screen size and colored distinctly based on which person it represents (specific colors for Matt, Nick, and Kim).
Physics Engine Integration: Handles the realistic movement, collision detection, and bouncing behavior of all circles against screen boundaries and each other.
Touch/Click Event Handling: Monitors for user interaction with the circles, registering successful "hits" and triggering the appropriate responses.
Visual Effects System: Implements various CSS transitions and animations for when circles are touched (fade-out, shrink, spin, etc.).
Audio Management: Handles the loading and playing of random sound files from the appropriate person's folder when their circle is touched.
Circle Regeneration Logic: Manages the delayed reappearance of circles in new, non-overlapping positions after being touched.
Game State Management: Controls the overall game flow, including initialization, countdown, difficulty progression, and game end conditions.
End-Game Transition: Handles the completion of the 20-second time limit and triggers the score submission process.
The Game page contains sophisticated JavaScript interop for handling the physics calculations and touch events with optimal performance. It preloads all necessary audio assets at initialization to prevent lag during gameplay. The difficulty scaling is implemented by subtly increasing circle movement speed as the timer counts down.

3. Leader.razor (Leaderboard Screen)
The Leader.razor page displays the top 10 highest scores achieved in the game. It features:

Header Section: Contains the title "Leaderboard" and a back button to return to the main menu.
Leaderboard Table: Displays a clean, formatted list of the top 10 scores with the following columns:
Rank (1-10)
Player Initials (3 letters)
Score
Date Achieved
Visual Styling: Implements a trophy or medal icon beside the top three positions to visually distinguish the highest achievements.
Empty State Handling: Shows appropriate messaging if fewer than 10 scores exist in the database.
Data Retrieval: Connects to the backend API to fetch the current leaderboard data when the page is loaded.
Responsive Layout: Ensures the leaderboard is clearly visible and properly formatted across different device sizes while maintaining portrait orientation.
Animation: Includes subtle entrance animations for each leaderboard entry to enhance visual appeal.
The Leader page focuses on clarity and readability, presenting achievement information in a straightforward manner that encourages competition among players.

4. HighScoreModal.razor (Component)
This modal component appears when a player achieves a score that ranks in the top 10. It includes:

Congratulatory Message: Announces the player's achievement with engaging text and possibly animation.
Score Display: Shows the player's final score prominently.
Initial Input: Provides a three-character input field for the player to enter their initials, with validation to ensure exactly three letters are entered.
Submit Button: Allows the player to confirm their initials and submit their score to the leaderboard.
Cancel Option: Provides a way for players to decline recording their score if desired.
Error Handling: Manages any issues with the submission process and displays appropriate feedback.
Keyboard Focus: Automatically focuses the input field and potentially triggers the mobile keyboard on touch devices.
This component handles the API communication necessary to submit the new high score to the backend service and updates the leaderboard accordingly.

5. GameOverModal.razor (Component)
This modal appears at the end of each game session when the 20-second timer expires and the player has not achieved a top 10 score. It includes:

Game Over Message: Indicates that the game session has ended.
Final Score Display: Shows the player's achieved score prominently.
Encouraging Message: Provides positive feedback regardless of performance.
Play Again Button: Allows the player to immediately start a new game session.
Return to Menu Button: Gives the option to return to the main menu instead.
Animation: Features entrance and exit animations to smoothly integrate with the game flow.
This component focuses on providing clear feedback and encouraging continued engagement with the game.

Backend Components
1. ScoreController.cs (API Controller)
This controller in the ASP.NET Core Web API project handles all leaderboard-related operations:

GET /api/scores: Retrieves the current top 10 scores from Azure Table Storage.
POST /api/scores: Receives and processes new score submissions, validating them against existing scores to determine if they qualify for the leaderboard.
Authentication/Authorization: Implements basic validation to prevent score tampering or unauthorized submissions.
Error Handling: Provides appropriate HTTP status codes and error messages for various failure scenarios.
Logging: Tracks API usage and potential issues for monitoring purposes.
2. ScoreService.cs (Azure Table Storage Service)
This service handles all direct interactions with Azure Table Storage:

Connection Management: Establishes and maintains the connection to the Azure Table Storage resource.
Data Access Operations: Implements the CRUD operations needed for leaderboard management.
Entity Mapping: Converts between the application's score model and the Azure Table Storage entity structure.
Partition Strategy: Implements best practices for partition and row keys to optimize performance.
Query Optimization: Ensures efficient retrieval of leaderboard data.
3. Models and DTOs
Various model classes support the application:

ScoreEntity: Represents a score record in the Azure Table Storage.
ScoreSubmissionDto: Data transfer object for new score submissions from the client.
LeaderboardEntryDto: Data transfer object for leaderboard entries returned to the client.