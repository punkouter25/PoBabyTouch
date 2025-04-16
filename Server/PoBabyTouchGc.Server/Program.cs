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

// Configure Azure Table Storage with Azurite for local development
string storageConnectionString = builder.Configuration.GetConnectionString("AzureTableStorage") 
    ?? "UseDevelopmentStorage=true"; // Default to Azurite if no connection string provided

builder.Services.AddSingleton(implementationFactory => {
    var tableServiceClient = new TableServiceClient(storageConnectionString);
    // Create the scores table if it doesn't exist
    var tableClient = tableServiceClient.GetTableClient("Scores");
    tableClient.CreateIfNotExists();
    return tableClient;
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
