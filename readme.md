# PoBabyTouchGc

PoBabyTouchGc is a simple web-based game where players click on moving circles to earn points within a time limit. After the game ends, players can submit their scores to a leaderboard. The application tracks high scores per game mode and allows players to view top scores and their rank.

## Key Features:
- **Interactive Gameplay:** Players click on dynamically moving circles.
- **Score Tracking:** Keeps track of the player's current score during gameplay.
- **Timer:** A countdown timer limits the game duration.
- **High Score System:** Players can submit their initials and score to a persistent leaderboard.
- **Leaderboard:** Displays top scores, allowing filtering by game mode.
- **Azure Table Storage Integration:** High scores are stored and retrieved from Azure Table Storage.
- **Blazor WebAssembly Frontend:** Provides a rich, interactive user interface in the browser.
- **ASP.NET Core Backend:** Serves the Blazor application and provides RESTful APIs for high score management.
- **Serilog Logging:** Comprehensive logging to console and file for diagnostics.

## How to Run the App:

The application consists of a Blazor WebAssembly client and an ASP.NET Core server.

### Prerequisites:
- .NET SDK (version 9.0 or later)
- Node.js and npm (for Mermaid CLI, if you want to generate diagrams)
- Azurite (for local Azure Table Storage emulation) or an Azure Storage Account connection string.

### Steps to Run:

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/punkouter25/PoBabyTouch.git
    cd PoBabyTouch
    ```

2.  **Install Azurite (if not already installed):**
    Azurite is used for local development of Azure Storage.
    ```bash
    npm install -g azurite
    ```

3.  **Start Azurite:**
    Open a terminal and run:
    ```bash
    azurite --silent
    ```
    This will start Azurite, emulating Azure Blob, Queue, and Table Storage. It will create `__blobstorage__`, `__queuestorage__`, and `__tablestorage__` directories in your current working directory.

4.  **Run the application:**
    Open a new terminal in the root directory of the project (`PoBabyTouchGc.sln` is located here) and run:
    ```bash
    dotnet run --project Server/PoBabyTouchGc.Server/PoBabyTouchGc.Server.csproj
    ```
    This will build and run the ASP.NET Core server, which also hosts the Blazor WebAssembly client.

5.  **Access the application:**
    Once the server starts, open your web browser and navigate to the URL provided in the console output (usually `https://localhost:7000` or `http://localhost:5000`).

## Diagrams:
The `Diagram` folder contains Mermaid diagrams (`.mmd` files) illustrating various aspects of the application, including:
- Flowchart
- Sequence Diagram
- Class Diagram
- Entity-Relationship (ER) Diagram
- State Diagram
- CSPROJ Dependency Diagram

To view these diagrams as SVG images, you can use the Mermaid CLI.
1.  **Install Mermaid CLI:**
    ```bash
    npm install -g @mermaid-js/mermaid-cli
    ```
2.  **Convert diagrams to SVG:**
    Navigate to the `Diagram` folder and run:
    ```bash
    mmdc -i YourDiagram.mmd -o YourDiagram.svg
    ```
    (Replace `YourDiagram.mmd` with the actual filename, or use a script to convert all of them.)
