namespace Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetApiKey()
        {
            return _configuration.GetSection(Declartions.ApiKeySettingName).Value;
        }
        public string GetAuthToken()
        {
            return _configuration["Auth:Token"]; 
        }
    }
}
