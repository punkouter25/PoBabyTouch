# Secret Management for PoBabyTouch

This document explains how to securely store and manage Azure connection strings and other secrets for local development.

## 🔐 Secret Storage Options

### Option 1: User Secrets (Recommended)

**Best for**: .NET developers, most secure, built into .NET tooling

```bash
# Navigate to the server project
cd Server/PoBabyTouchGc.Server

# Set the Azure Table Storage connection string
dotnet user-secrets set "ConnectionStrings:AzureTableStorage" "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=posharedtablestorage;AccountKey=YOUR_KEY_HERE;BlobEndpoint=https://posharedtablestorage.blob.core.windows.net/;FileEndpoint=https://posharedtablestorage.file.core.windows.net/;QueueEndpoint=https://posharedtablestorage.queue.core.windows.net/;TableEndpoint=https://posharedtablestorage.table.core.windows.net/"

# List all user secrets
dotnet user-secrets list

# Remove a user secret
dotnet user-secrets remove "ConnectionStrings:AzureTableStorage"
```

**Advantages:**
- ✅ Never committed to source control
- ✅ Encrypted storage on your machine
- ✅ Built into .NET tooling
- ✅ Works seamlessly with ASP.NET Core configuration

### Option 2: Secrets JSON File

**Best for**: Teams that prefer file-based configuration

1. Copy the template:
   ```bash
   cp appsettings.secrets.json.template appsettings.secrets.json
   ```

2. Edit `appsettings.secrets.json` with your actual values:
   ```json
   {
     "ConnectionStrings": {
       "AzureTableStorage": "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=posharedtablestorage;AccountKey=YOUR_ACTUAL_KEY;..."
     }
   }
   ```

**Advantages:**
- ✅ Easy to understand and edit
- ✅ Automatically ignored by Git
- ✅ Can store multiple secrets
- ✅ Supports JSON hierarchy

### Option 3: Environment Variables

**Best for**: Containerized environments, CI/CD pipelines

1. Copy the environment template:
   ```bash
   cp .env.template .env.local
   ```

2. Edit `.env.local` with your values:
   ```
   ConnectionStrings__AzureTableStorage=DefaultEndpointsProtocol=https;...
   ```

3. Load environment variables before running:
   ```bash
   # PowerShell
   Get-Content .env.local | ForEach-Object { $env:($_.Split('=')[0]) = $_.Split('=')[1] }
   
   # Bash
   export $(cat .env.local | xargs)
   ```

**Advantages:**
- ✅ Works with any technology stack
- ✅ Standard for containerized applications
- ✅ Easy CI/CD integration

## 🏃‍♂️ Quick Start

1. **For Azure Storage**: Use User Secrets (Option 1)
   ```bash
   cd Server/PoBabyTouchGc.Server
   dotnet user-secrets set "ConnectionStrings:AzureTableStorage" "YOUR_CONNECTION_STRING"
   ```

2. **For Azurite (Local)**: No secrets needed!
   - The app automatically uses `UseDevelopmentStorage=true` when no secrets are found
   - Just start Azurite: `azurite --silent --location AzuriteData`

## 🔧 Configuration Priority

The application loads configuration in this order (last wins):

1. `appsettings.json`
2. `appsettings.Development.json` (in Development environment)
3. `appsettings.secrets.json` (if exists)
4. User Secrets (in Development environment)
5. Environment Variables
6. Command Line Arguments

## 🚫 What NOT to Do

- ❌ Never commit connection strings to Git
- ❌ Never put secrets in `appsettings.json` or `appsettings.Development.json`
- ❌ Never share secrets in chat or email
- ❌ Never hardcode secrets in source code

## 🛡️ Security Best Practices

1. **Rotate Keys Regularly**: Change your Azure Storage keys periodically
2. **Use Minimal Permissions**: Only grant necessary access permissions
3. **Monitor Access**: Review Azure Storage access logs
4. **Use Managed Identity**: In production, prefer Managed Identity over connection strings

## 🆘 Troubleshooting

**Problem**: App can't connect to Azure Storage
**Solution**: 
1. Check user secrets: `dotnet user-secrets list`
2. Verify connection string format
3. Test with Azurite: Remove secrets and app will auto-fallback

**Problem**: Secrets showing in Git
**Solution**: 
1. Remove from staging: `git reset HEAD -- appsettings.Development.json`
2. Add to .gitignore (already done)
3. Use `git filter-branch` to remove from history if already committed
