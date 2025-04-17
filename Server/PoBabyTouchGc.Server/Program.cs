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
builder.Services.AddSingleton(implementationFactory => {
    try 
    {
        Log.Information("Initializing Azure Table Storage client");
        var tableServiceClient = new TableServiceClient(storageConnectionString);
        // Create the scores table if it doesn't exist
        var tableClient = tableServiceClient.GetTableClient("Scores");
        tableClient.CreateIfNotExists();
        Log.Information("Successfully connected to Azure Table Storage and verified 'Scores' table");
        return tableClient;
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to initialize Azure Table Storage client");
        throw; // Rethrow to prevent application from starting with invalid configuration
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
builder.Services.AddScoped<IScoreService, ScoreService>();

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
