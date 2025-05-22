# PoBabyTouchGc Application Architecture

```mermaid
C4Container
    title PoBabyTouchGc Application Architecture

    Person(user, "User", "Plays the game and views leaderboards/diagnostics.")

    Container(client, "PoBabyTouchGc.Client", "Blazor WebAssembly App", "The client-side application running in the user's browser.") {
        Component(client_pages, "Pages", "Blazor Components (.razor)", "User interface for Home, Game, Leaderboard, and Diagnostics.")
        Component(client_layout, "Layout", "Blazor Components (.razor)", "Defines the overall UI structure and navigation.")
        Component(client_js, "Game JavaScript", "game.js", "Handles client-side game logic and interactions.")
        Component(client_program, "Program.cs", "C#", "Entry point for the Blazor application.")
    }

    Container(server, "PoBabyTouchGc.Server", "ASP.NET Core Web API", "The backend API responsible for game logic, data access, and diagnostics.") {
        Component(server_controllers, "ScoresController", "C# API Controller", "Exposes endpoints for submitting scores and retrieving leaderboard data.")
        Component(server_services, "ScoreService", "C# Service", "Implements business logic for managing game scores.")
        Component(server_iservice, "IScoreService", "C# Interface", "Contract for the score management service.")
        Component(server_program, "Program.cs", "C#", "Entry point for the ASP.NET Core application, handles configuration and dependency injection.")
    }

    Container(shared, "PoBabyTouchGc.Shared", "C# Class Library", "Contains data models and shared definitions used by both client and server.") {
        Component(shared_models, "Models", "C# Classes", "LeaderboardEntry, ScoreEntity, ScoreSubmission - define the structure of data exchanged.")
    }

    Container(tests, "PoBabyTouchGc.Tests", "XUnit Test Project", "Contains automated unit and integration tests for the server-side components.") {
        Component(test_classes, "Test Classes", "C# Classes", "Verifies the correctness of ScoreService and ScoresController logic.")
    }

    SystemDb(azure_table_storage, "Azure Table Storage", "NoSQL Database", "Cloud-based persistent storage for game scores (production).")
    SystemDb(azurite, "Azurite", "Local Emulator", "Local development emulator for Azure Table Storage.")

    Rel(user, client, "Interacts with")
    Rel(client_pages, server_controllers, "Makes API calls to", "HTTP/JSON")
    Rel(server_controllers, server_services, "Uses")
    Rel(server_services, azure_table_storage, "Reads/Writes data to", "Azure Table Storage SDK")
    Rel(server_services, azurite, "Reads/Writes data to (Local Dev)", "Azure Table Storage SDK")
    Rel(client_pages, shared_models, "Uses")
    Rel(server_services, shared_models, "Uses")
    Rel(server_controllers, shared_models, "Uses")
    Rel(client_pages, client_js, "Invokes functions in")
    Rel(tests, server_services, "Tests")
    Rel(tests, server_controllers, "Tests")

    Note(log_file_note, "log.txt: Diagnostic log file, created new with each run, used for debugging and diagnostics.")
    Rel(server, log_file_note, "Writes logs to")
    Rel(client, log_file_note, "Writes logs to (via server proxy or direct JS logging)")
