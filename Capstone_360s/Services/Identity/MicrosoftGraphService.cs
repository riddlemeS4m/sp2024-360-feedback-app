using Capstone_360s.Interfaces.IService;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Capstone_360s.Services.Identity
{
    public class MicrosoftGraphService : IMicrosoftGraph
    {
        private readonly GraphServiceClient _graphClient;

        public MicrosoftGraphService(GraphServiceClient graphClient)
        {
            _graphClient = graphClient;
        }

        public async Task<User> GetUserIdByEmailAsync(string email)
        {
            try
            {
                // Use the pre-configured graph client directly
                var user = await _graphClient.Users[email].Request().GetAsync();
                return user ?? throw new ArgumentNullException($"Microsoft couldn't find the user '{email}'.");
            }
            catch (MsalUiRequiredException ex)
            {
                // Force re-authentication
                throw new UnauthorizedAccessException("User session expired or additional consent required. Please log in again.", ex);
            }
        }
    }
}
