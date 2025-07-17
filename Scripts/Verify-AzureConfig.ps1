#!/usr/bin/env pwsh

# Azure App Service Configuration Verification Script
# This script verifies that the Azure Table Storage connection string is properly configured

Write-Host "🔍 Verifying Azure App Service Configuration for PoBabyTouch..." -ForegroundColor Cyan

# Check if logged into Azure
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "❌ Not logged into Azure CLI. Please run 'az login'" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Logged into Azure as: $($account.user.name)" -ForegroundColor Green

# Check App Service exists
$appService = az webapp show --name "PoBabyTouch" --resource-group "pobabytouch" 2>$null | ConvertFrom-Json
if (-not $appService) {
    Write-Host "❌ App Service 'PoBabyTouch' not found" -ForegroundColor Red
    exit 1
}

Write-Host "✅ App Service found: $($appService.name) in $($appService.location)" -ForegroundColor Green

# Check connection string configuration
$connectionString = az webapp config appsettings list --name "PoBabyTouch" --resource-group "pobabytouch" --query "[?name=='ConnectionStrings__AzureTableStorage'].value" --output tsv

if ($connectionString) {
    Write-Host "✅ Azure Table Storage connection string is configured" -ForegroundColor Green
    
    # Verify it's a proper Azure Storage connection string
    if ($connectionString -like "*AccountName=posharedtablestorage*" -and $connectionString -like "*AccountKey=*") {
        Write-Host "✅ Connection string format appears correct" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Connection string format may be incorrect" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ Azure Table Storage connection string not found" -ForegroundColor Red
    exit 1
}

# Check App Service status
$appService = az webapp show --name "PoBabyTouch" --resource-group "pobabytouch" --query "{state:state, defaultHostName:defaultHostName}" | ConvertFrom-Json

Write-Host "📊 App Service Status:" -ForegroundColor Cyan
Write-Host "   State: $($appService.state)" -ForegroundColor White
Write-Host "   URL: https://$($appService.defaultHostName)" -ForegroundColor White

if ($appService.state -eq "Running") {
    Write-Host "✅ App Service is running" -ForegroundColor Green
} else {
    Write-Host "⚠️  App Service state: $($appService.state)" -ForegroundColor Yellow
}

Write-Host "`n🎉 Configuration verification complete!" -ForegroundColor Green
Write-Host "Your App Service should now be using the Azure Table Storage connection string." -ForegroundColor White
