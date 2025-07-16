# Azure App Service Configuration Script
# This script configures the Azure App Service with the proper connection string for Azure Table Storage

param(
    [Parameter(Mandatory=$true)]
    [string]$AppServiceName,
    
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$StorageAccountName,
    
    [Parameter(Mandatory=$true)]
    [string]$StorageAccountKey,
    
    [Parameter(Mandatory=$false)]
    [string]$SubscriptionId
)

# Login to Azure if not already logged in
$context = Get-AzContext
if (!$context) {
    Write-Host "Logging into Azure..." -ForegroundColor Yellow
    Connect-AzAccount
}

# Set subscription if provided
if ($SubscriptionId) {
    Set-AzContext -SubscriptionId $SubscriptionId
}

# Construct the connection string
$connectionString = "DefaultEndpointsProtocol=https;AccountName=$StorageAccountName;AccountKey=$StorageAccountKey;EndpointSuffix=core.windows.net"

Write-Host "Configuring Azure App Service: $AppServiceName" -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Green
Write-Host "Storage Account: $StorageAccountName" -ForegroundColor Green

# Set the connection string in App Service
try {
    # Method 1: Set as App Setting (recommended for connection strings)
    Set-AzWebApp -ResourceGroupName $ResourceGroupName -Name $AppServiceName -AppSettings @{
        "ConnectionStrings:AzureTableStorage" = $connectionString
    }
    
    # Method 2: Set as Connection String (Azure specific format)
    Set-AzWebAppConnectionString -ResourceGroupName $ResourceGroupName -Name $AppServiceName -ConnectionStringName "AzureTableStorage" -ConnectionString $connectionString -ConnectionStringType "Custom"
    
    Write-Host "Successfully configured connection string for $AppServiceName" -ForegroundColor Green
    
    # Restart the app service to apply changes
    Write-Host "Restarting App Service to apply changes..." -ForegroundColor Yellow
    Restart-AzWebApp -ResourceGroupName $ResourceGroupName -Name $AppServiceName
    
    Write-Host "App Service restarted successfully" -ForegroundColor Green
    
    # Display the app service URL
    $webapp = Get-AzWebApp -ResourceGroupName $ResourceGroupName -Name $AppServiceName
    Write-Host "App Service URL: https://$($webapp.DefaultHostName)" -ForegroundColor Cyan
    Write-Host "Health Check URL: https://$($webapp.DefaultHostName)/api/health" -ForegroundColor Cyan
    Write-Host "API Diagnostics URL: https://$($webapp.DefaultHostName)/api/highscores/diagnostics" -ForegroundColor Cyan
    
} catch {
    Write-Error "Failed to configure App Service: $($_.Exception.Message)"
    exit 1
}

Write-Host "Configuration complete! Test the health endpoint to verify the connection." -ForegroundColor Green
