using Capstone_360s.Models;
using Capstone_360s.Utilities;
using Capstone_360s.Interfaces.IService;

namespace Capstone_360s.Services.Configuration
{
    public class CustomConfigurationService : IConfigureEnvironment
    {
        public string SendGridKey { get; private set; }
        public string MicrosoftClientId { get; private set; }
        public string MicrosoftTenantId { get; private set; }
        public string MicrosoftClientSecret { get; private set; }
        public string MicrosoftDomain { get; private set; }
        public string MicrosoftTokenAcquisitionScopes { get; private set; }
        public string MicrosoftGraphScopes { get; private set; }
        public string MicrosoftInstance { get; private set; }
        public string MicrosoftGraphBaseUrl { get; private set; }
        public string MicrosoftCallbackPath { get; private set; }
        public string GoogleCredentials { get; private set; }
        public string FeedbackDbConnection { get; private set; }
        public string RolesDbConnection { get; private set; }
        public string SystemAdministrator { get; set; }
        public string ProgramManager { get; private set; }
        public string Instructor { get; private set; }
        public string TeamLead { get; private set; }
        public string Member { get; private set; }
        public string CapstoneOrg { get; private set; }

        public CustomConfigurationService(IConfiguration configuration, string env)
        {
            var configMappings = new Dictionary<string, string>
            {
                { nameof(SendGridKey), CustomConfiguration.SendGridKey },
                { nameof(FeedbackDbConnection), CustomConfiguration.FeedbackDbConnectionString(env) },
                { nameof(RolesDbConnection), CustomConfiguration.RolesDbConnectionString(env) },
                { nameof(MicrosoftClientId), CustomConfiguration.MicrosoftClientIdKey },
                { nameof(MicrosoftTenantId), CustomConfiguration.MicrosoftTenantIdKey },
                { nameof(MicrosoftClientSecret), CustomConfiguration.MicrosoftClientSecretKey },
                { nameof(MicrosoftDomain), CustomConfiguration.MicrosoftDomainKey },
                { nameof(MicrosoftTokenAcquisitionScopes), CustomConfiguration.MicrosoftTokenAcquisitionScopesKey },
                { nameof(MicrosoftGraphScopes), CustomConfiguration.MicrosoftGraphScopesKey },
                { nameof(MicrosoftInstance), CustomConfiguration.MicrosoftInstanceKey },
                { nameof(MicrosoftGraphBaseUrl), CustomConfiguration.MicrosoftGraphBaseUrlKey },
                { nameof(MicrosoftCallbackPath), CustomConfiguration.MicrosoftCallbackPathKey },
                { nameof(SystemAdministrator), CustomConfiguration.SystemAdministratorRoleKey},
                { nameof(ProgramManager), CustomConfiguration.ProgramManagerRoleKey },
                { nameof(Instructor), CustomConfiguration.InstructorRoleKey },
                { nameof(TeamLead), CustomConfiguration.TeamLeadRoleKey },
                { nameof(Member), CustomConfiguration.MemberRoleKey },
                { nameof(CapstoneOrg), CustomConfiguration.CapstoneOrganizationFunctionalityKey }
            };

            // Assign values dynamically
            foreach (var mapping in configMappings)
            {
                GetType().GetProperty(mapping.Key)?.SetValue(this, configuration[mapping.Value]);
            }

            GoogleCredentials = GoogleDriveUtility.SerializeCredentials(configuration.GetSection(CustomConfiguration.GoogleDriveCredentialSection));

            ValidateConfiguration();
        }

        private void ValidateConfiguration()
        {
            var missingKeys = GetType()
                .GetProperties()
                .Where(p => string.IsNullOrEmpty(p.GetValue(this) as string))
                .Select(p => p.Name)
                .ToList();

            if (missingKeys.Any())
            {
                throw new InvalidOperationException($"Missing configuration keys: {string.Join(", ", missingKeys)}");
            }
        }
    }
}
