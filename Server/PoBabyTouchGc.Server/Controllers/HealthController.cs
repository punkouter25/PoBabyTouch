using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using System.Net.NetworkInformation;

namespace PoBabyTouchGc.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger<HealthController> _logger;

        public HealthController(TableServiceClient tableServiceClient, ILogger<HealthController> logger)
        {
            _tableServiceClient = tableServiceClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetHealthStatus()
        {
            var healthStatus = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Application = "PoBabyTouchGc",
                Version = "1.0.0",
                Dependencies = await CheckDependencies()
            };

            _logger.LogInformation("Health check performed at {Timestamp}", healthStatus.Timestamp);

            return Ok(healthStatus);
        }

        private async Task<object> CheckDependencies()
        {
            var dependencies = new
            {
                AzureTableStorage = await CheckAzureTableStorage(),
                InternetConnectivity = await CheckInternetConnectivity(),
                Self = CheckSelf()
            };

            return dependencies;
        }

        private async Task<object> CheckAzureTableStorage()
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("PoBabyTouchGcHighScores");
                await tableClient.GetEntityAsync<TableEntity>("test", "test");
                return new { Status = "Connected", Message = "Azure Table Storage is accessible" };
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                // Entity not found is expected - this means the table is accessible
                return new { Status = "Connected", Message = "Azure Table Storage is accessible" };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Azure Table Storage health check failed");
                return new { Status = "Failed", Message = ex.Message };
            }
        }

        private async Task<object> CheckInternetConnectivity()
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                var response = await httpClient.GetAsync("https://azure.microsoft.com/");

                return new
                {
                    Status = response.IsSuccessStatusCode ? "Connected" : "Failed",
                    Message = response.IsSuccessStatusCode ? "Internet connectivity verified" : $"HTTP {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Internet connectivity check failed");
                return new { Status = "Failed", Message = ex.Message };
            }
        }

        private object CheckSelf()
        {
            // Basic self check - if we can execute this, the API is healthy
            return new { Status = "Healthy", Message = "API is responding correctly" };
        }
    }
}
