# PoBabyTouchGc

This is a simple baby touch game built with Blazor WebAssembly and .NET. The game is designed to be a fun and interactive way for babies to learn about cause and effect.

## Features

*   **Simple Gameplay:** The game is easy to play. Just touch the screen to make things happen.
*   **Cute Graphics:** The game features cute and colorful graphics that are sure to capture your baby's attention.
*   **Fun Sounds:** The game includes fun and engaging sounds that will keep your baby entertained.

## Getting Started

To get started, you'll need to have the following installed:

*   [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/)
*   [Azurite](https://docs.microsoft.com/azure/storage/common/storage-use-azurite)

### Setup

1.  Clone the repository:
    ```bash
    git clone https://github.com/your-username/PoBabyTouchGc.git
    ```
2.  Open the solution in Visual Studio or Visual Studio Code.
3.  Start Azurite.
4.  **Configure Azure Connection String (for production):**
    For production deployment, set the Azure Storage connection string using:
    ```bash
    cd Server/PoBabyTouchGc.Server
    dotnet user-secrets set "ConnectionStrings:AzureTableStorage" "your_azure_connection_string_here"
    ```
5.  Press F5 to run the application.

**Note:** The project uses Azurite for local development (configured in appsettings.Development.json) and requires an Azure Storage connection string for production (use user secrets or Azure App Service configuration).

The application will open in your default browser. You can now play the game.
