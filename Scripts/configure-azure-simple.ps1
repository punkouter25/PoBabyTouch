# Simple script to configure Azure App Service with Table Storage
# Run this in Azure Cloud Shell or with Azure CLI installed

# Variables - UPDATE THESE WITH YOUR ACTUAL VALUES
$resourceGroupName = "your-resource-group-name"
$appServiceName = "pobabytouch"
$storageAccountName = "pobabytouch"  # Your storage account name

# Get the storage account key
Write-Host "Getting storage account key..." -ForegroundColor Yellow
$storageKey = az storage account keys list --resource-group $resourceGroupName --account-name $storageAccountName --query "[0].value" --output tsv

if (!$storageKey) {
    Write-Error "Failed to get storage account key"
    exit 1
}

# Create the connection string
$connectionString = "DefaultEndpointsProtocol=https;AccountName=$storageAccountName;AccountKey=$storageKey;EndpointSuffix=core.windows.net"

Write-Host "Configuring App Service connection string..." -ForegroundColor Yellow

# Set the connection string using Azure CLI
az webapp config connection-string set --resource-group $resourceGroupName --name $appServiceName --connection-string-type "Custom" --settings AzureTableStorage="$connectionString"

# Also set it as an app setting for compatibility
az webapp config appsettings set --resource-group $resourceGroupName --name $appServiceName --settings "ConnectionStrings:AzureTableStorage=$connectionString"

# Restart the app service
Write-Host "Restarting App Service..." -ForegroundColor Yellow
az webapp restart --resource-group $resourceGroupName --name $appServiceName

Write-Host "Configuration complete!" -ForegroundColor Green
Write-Host "Test URLs:" -ForegroundColor Cyan
Write-Host "  Health Check: https://$appServiceName.azurewebsites.net/api/health" -ForegroundColor Cyan
Write-Host "  Diagnostics:  https://$appServiceName.azurewebsites.net/api/highscores/diagnostics" -ForegroundColor Cyan
