using InventoryService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace InventoryService.Handler
{
    public class ApiAuthHelperer : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IApiKeyService _apiKeyService;

        // Define constants for claim types and values
        private const string ClaimTypeName = ClaimTypes.Name;
        private const string ClaimTypeRole = ClaimTypes.Role;
        private const string ClaimValueName = "Name"; // Replace with actual name value if needed
        private const string ClaimValueRole = "User"; // Replace with actual role value if needed

        public ApiAuthHelperer(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IApiKeyService apiKeyService)
            : base(options, logger, encoder, clock)
        {
            _apiKeyService = apiKeyService;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Authenticate the API key
            var isApiKeyValid = _apiKeyService.Authenticate();
            if (!isApiKeyValid)
            {
                // Log the failure
                Logger.LogWarning("Authentication failed: Invalid API Key");
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
            }

            // Create claims for the authenticated user
            var claims = new[]
            {
                new Claim(ClaimTypeName, ClaimValueName),
                new Claim(ClaimTypeRole, ClaimValueRole)
            };

            // Create the identity and principal
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            // Create the authentication ticket
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            // Return success result with the authentication ticket
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
