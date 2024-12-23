using Capstone_360s.Interfaces.IService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger<MicrosoftGraphService> _logger;

        public MicrosoftGraphService(
            ITokenAcquisition tokenAcquisition,
            GraphServiceClient graphClient,
            IHttpContextAccessor httpContext,
            ILogger<MicrosoftGraphService> logger)
        {
            _tokenAcquisition = tokenAcquisition;
            _graphClient = graphClient;
            _httpContext = httpContext;
            _logger = logger;
        }

        public async Task<User> GetUserIdByEmailAsync(string email)
        {
            // await _tokenRefreshLock.WaitAsync();

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

                // _graphClient.AuthenticationProvider = new DelegateAuthenticationProvider(request =>
                // {
                //     request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", NewToken);
                //     return Task.CompletedTask;
                // });

                var user = await _graphClient.Users[email].Request().GetAsync();
                return user;
            }
            catch (MsalUiRequiredException ex)
            {
                // Force re-authentication if consent or interaction is required
                _logger.LogError("MsalUiRequiredException: {0}", ex.Message);
                throw new UnauthorizedAccessException("User session expired or additional consent required. Please log in again.", ex);
            }
        }

        private async Task RefreshAccessTokenAsync()
        {
            try
            {
                var newAccessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(
                    scopes: new[] { "user.readbasic.all", "offline_access" },
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

        // private async Task<string> RefreshAccessTokenAsync()
        // {
        //     string idClaim ="";

        //     try
        //     {
        //         var authResult = await _httpContext.HttpContext.AuthenticateAsync(
        //             CookieAuthenticationDefaults.AuthenticationScheme
        //         );

        //         var user = authResult.Principal;

        //         if (user == null || !user.Identity.IsAuthenticated)
        //         {
        //             throw new UnauthorizedAccessException("User session expired or not authenticated.");
        //         }

        //         idClaim = user.FindFirst(x => x.Type == "uid")?.Value;

        //         var accessToken = authResult.Properties?.GetTokenValue("access_token");
        //         var refreshToken = authResult.Properties?.GetTokenValue("refresh_token");

        //         if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
        //         {
        //             throw new UnauthorizedAccessException("Access or refresh token not found.");
        //         }

        //         var newAccessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(
        //             scopes: new[] { "User.Read.All", "offline_access" },
        //             authenticationScheme: OpenIdConnectDefaults.AuthenticationScheme,
        //             user: user
        //         );

        //         _logger.LogInformation("Access token acquired successfully.");
        //         return newAccessToken;
        //     }
        //     catch (MicrosoftIdentityWebChallengeUserException ex)
        //     {
        //         _logger.LogError("Failed to acquire token directly from MSAL.");
        //         throw new UnauthorizedAccessException("Failed to acquire token directly from MSAL.");
        //     }
        // }
    }
}
