using Capstone_360s.Models;
using Capstone_360s.Utilities;

namespace Capstone_360s.Services.Configuration
{
    public class CustomConfigurationService
    {
        public string SendGridKey { get; }
        public string MicrosoftClientId { get; }
        public string MicrosoftTenantId { get; }
        public string MicrosoftClientSecret { get; }
        public string MicrosoftDomain { get; }
        public string MicrosoftTokenAcquisitionScopes { get; }
        public string MicrosoftGraphScopes { get; }
        public string MicrosoftInstance { get; }
        public string MicrosoftGraphBaseUrl { get; }
        public string MicrosoftCallbackPath { get;  }
        public string GoogleCredentials { get; }
        public string FeedbackDbConnection { get; }
        public string RolesDbConnection { get; }
        public string Administrator { get; }
        public string Sponsor { get; }
        public string Lead { get; }
        public string Member { get; }
        public string CapstoneOrg { get; }

        public CustomConfigurationService(IConfiguration configuration, string env)
        {
            SendGridKey = configuration[CustomConfiguration.SendGridKey];

            FeedbackDbConnection = configuration[CustomConfiguration.FeedbackDbConnectionString(env)];
            RolesDbConnection = configuration[CustomConfiguration.RolesDbConnectionString(env)];

            MicrosoftClientId = configuration[CustomConfiguration.MicrosoftClientId];
            MicrosoftTenantId = configuration[CustomConfiguration.MicrosoftTenantId];
            MicrosoftClientSecret = configuration[CustomConfiguration.MicrosoftClientSecret];
            MicrosoftDomain = configuration[CustomConfiguration.MicrosoftDomain];
            MicrosoftTokenAcquisitionScopes = configuration[CustomConfiguration.MicrosoftTokenAcquisitionScopes];
            MicrosoftGraphScopes = configuration[CustomConfiguration.MicrosoftGraphScopes];
            MicrosoftInstance = configuration[CustomConfiguration.MicrosoftInstance];
            MicrosoftGraphBaseUrl = configuration[CustomConfiguration.MicrosoftGraphBaseUrl];
            MicrosoftCallbackPath = configuration[CustomConfiguration.MicrosoftCallbackPath];

            Administrator = configuration[CustomConfiguration.AdministratorRole];
            Sponsor = configuration[CustomConfiguration.SponsorRole];
            Lead = configuration[CustomConfiguration.LeadRole];
            Member = configuration[CustomConfiguration.MemberRole];

            GoogleCredentials = GoogleDriveUtility.SerializeCredentials(configuration.GetSection(CustomConfiguration.GoogleDriveCredentials));

            CapstoneOrg = configuration[CustomConfiguration.CapstoneOrg];

            ValidateConfiguration();
        }

        private void ValidateConfiguration()
        {
            foreach (var property in GetType().GetProperties())
            {
                var value = property.GetValue(this) as string;
                if (string.IsNullOrEmpty(value))
                {
                    throw new InvalidOperationException($"Configuration key '{property.Name}' is missing or empty.");
                }
            }
        }
    }
}
