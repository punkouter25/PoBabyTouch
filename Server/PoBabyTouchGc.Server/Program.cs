using Microsoft.Extensions.Azure;
using Azure.Data.Tables;
using PoBabyTouchGc.Server.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Configure Azure Table Storage
// In development: Uses the UserSecrets connection string or Azurite
// In production: Uses the connection string from App Service config
string storageConnectionString = builder.Configuration.GetConnectionString("AzureTableStorage")
    ?? Environment.GetEnvironmentVariable("AzureTableStorage")
    ?? "UseDevelopmentStorage=true"; // Default to Azurite if no connection string provided

Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
Log.Information("Using {StorageType} for Azure Table Storage", 
    storageConnectionString == "UseDevelopmentStorage=true" ? "Azurite (local)" : "Azure Storage");

// Configure table storage client
// Register TableServiceClient for high scores
builder.Services.AddSingleton<TableServiceClient>(implementationFactory => {
    try 
    {
        Log.Information("Initializing Azure TableServiceClient for high scores");
        var tableServiceClient = new TableServiceClient(storageConnectionString);
        // Ensure the high scores table exists
        tableServiceClient.GetTableClient("PoBabyTouchGcHighScores").CreateIfNotExists();
        Log.Information("Successfully initialized TableServiceClient and verified 'PoBabyTouchGcHighScores' table");
        return tableServiceClient;
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to initialize TableServiceClient");
        throw;
    }
});

// Add CORS policy to allow the Blazor client to call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add our custom services
builder.Services.AddScoped<IHighScoreService, HighScoreService>();

// Configure response compression
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseBlazorFrameworkFiles();
app.UseCors("CorsPolicy");

app.UseRouting();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
