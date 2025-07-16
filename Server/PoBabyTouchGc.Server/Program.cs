using Microsoft.Extensions.Azure;
using Azure.Data.Tables;
using PoBabyTouchGc.Server.Services;
using PoBabyTouchGc.Server.Repositories;
using PoBabyTouchGc.Server.Middleware;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using System.IO;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters; // Add this back

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for verbose logging in development
var logPath = Path.Combine(AppContext.BaseDirectory, "log.txt");

builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, shared: true, flushToDiskInterval: TimeSpan.FromSeconds(1))
    .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces));

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING");
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PoBabyTouchGc API",
        Version = "v1",
        Description = "API for PoBabyTouchGc - Baby Touch Game"
    });
});

builder.Services.AddControllers();

// Configure Azure Table Storage
// In development: Uses the UserSecrets connection string or Azurite
// In production: Uses the connection string from App Service config
string storageConnectionString = builder.Configuration.GetConnectionString("AzureTableStorage")
    ?? Environment.GetEnvironmentVariable("AzureTableStorage")
    ?? Environment.GetEnvironmentVariable("CUSTOMCONNSTR_AzureTableStorage") // Azure App Service format
    ?? Environment.GetEnvironmentVariable("SQLAZURECONNSTR_AzureTableStorage") // Alternative format
    ?? "UseDevelopmentStorage=true"; // Default to Azurite if no connection string provided

Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
Log.Information("Using {StorageType} for Azure Table Storage",
    storageConnectionString == "UseDevelopmentStorage=true" ? "Azurite (local)" : "Azure Storage");

// Configure table storage client
// Register TableServiceClient for high scores
builder.Services.AddSingleton<TableServiceClient>(implementationFactory =>
{
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
// Applying Repository Pattern for data access abstraction
builder.Services.AddScoped<IHighScoreRepository, AzureTableHighScoreRepository>();
// Applying Strategy Pattern for validation logic
builder.Services.AddScoped<IHighScoreValidationService, HighScoreValidationService>();
builder.Services.AddScoped<HighScoreValidationService>();
// Applying Service Layer Pattern for business logic
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

    // Enable Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PoBabyTouchGc API V1");
        c.RoutePrefix = "swagger"; // Accessible at /swagger
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseResponseCompression();

// Global Exception Handler Middleware - handles all unhandled exceptions
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

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
