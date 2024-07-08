using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace InventoryService.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyService> _logger;

        public ApiKeyService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<ApiKeyService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        public bool Authenticate()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                _logger.LogError("HttpContext is null. Cannot authenticate API key.");
                return false;
            }

            // Attempt to retrieve the API key from the request headers
            if (!context.Request.Headers.TryGetValue(Declartions.ApiKeyHeaderName, out var apiKey))
            {
                _logger.LogWarning("API key header not found in the request.");
                return false;
            }

            // Retrieve the expected API key from configuration
            var expectedApiKey = _configuration[Declartions.ApiKeySettingsKey];
            if (expectedApiKey == null)
            {
                _logger.LogError("API key configuration is missing.");
                return false;
            }

            // Check if the provided API key matches the expected API key
            var isAuthenticated = apiKey == expectedApiKey;
            if (!isAuthenticated)
            {
                _logger.LogWarning("Invalid API key provided.");
            }

            return isAuthenticated;
        }
    }
}
