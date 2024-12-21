using Capstone_360s.Interfaces.IService;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace Capstone_360s.Services.Identity
{
    public class MicrosoftGraphService : IMicrosoftGraph
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger<MicrosoftGraphService> _logger;

        public MicrosoftGraphService(GraphServiceClient graphClient, ITokenAcquisition tokenAcquisition, ILogger<MicrosoftGraphService> logger)
        {
            _graphClient = graphClient;
            _tokenAcquisition = tokenAcquisition;
            _logger = logger;
        }

        public async Task<User> GetUserIdByEmailAsync(string email)
        {
            try
            {
                // Attempt to get user info through Graph API
                var user = await _graphClient.Users[email].Request().GetAsync();
                return user ?? throw new ArgumentNullException($"Microsoft couldn't find the user '{email}'.");
            }
            catch (ServiceException ex) //when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Token expired or invalid. Attempting to refresh...");

                // Acquire a fresh token and retry the request
                await RefreshAccessTokenAsync();
                var user = await _graphClient.Users[email].Request().GetAsync();
                return user;
            }
            catch (MsalUiRequiredException ex)
            {
                // Force re-authentication if consent or interaction is required
                throw new UnauthorizedAccessException("User session expired or additional consent required. Please log in again.", ex);
            }
        }

        private async Task RefreshAccessTokenAsync()
        {
            try
            {
                var newAccessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(
                    scopes: new[] { "user.readbasic.all" },
                    authenticationScheme: OpenIdConnectDefaults.AuthenticationScheme    
                );
                _graphClient.AuthenticationProvider = new DelegateAuthenticationProvider(request =>
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newAccessToken);
                    return Task.CompletedTask;
                });

                _logger.LogInformation("Access token successfully refreshed.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to refresh access token. User may need to log in again.");
                throw new UnauthorizedAccessException("Re-authentication required.", ex);
            }
        }
    }
}
