namespace Client.Services
{
    public interface IAuthService
    {
        string GetApiKey();
        string GetAuthToken();
    }
}