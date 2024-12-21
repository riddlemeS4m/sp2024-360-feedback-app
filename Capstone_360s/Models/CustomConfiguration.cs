namespace Capstone_360s.Models
{
    public class CustomConfiguration
    {
        public const string Environment = "ASPNETCORE_ENVIRONMENT";
        public const string SendGridKey = "SendGrid";
        public const string MicrosoftClientId = "Authentication:Microsoft:ClientId";
        public const string MicrosoftTenantId = "Authentication:Microsoft:TenantId";
        public const string MicrosoftClientSecret = "Authentication:Microsoft:ClientSecret";
        public const string MicrosoftDomain = "Authentication:Microsoft:Domain";
        public const string MicrosoftTokenAcquisitionScopes = "Authentication:Microsoft:Scopes:Token";
        public const string MicrosoftGraphScopes = "Authentication:Microsoft:Scopes:Graph";
        public const string MicrosoftInstance = "Authentication:Microsoft:Instance";
        public const string MicrosoftGraphBaseUrl = "Authentication:Microsoft:GraphBaseUrl";
        public const string MicrosoftCallbackPath = "Authentication:Microsoft:CallbackPath";
        public const string ConnectionStringPrefix = "ConnectionStrings:";
        public const string GoogleDriveCredentials = "GoogleCredential";
        public const string AdministratorRole = "Roles:Administrator";
        public const string SponsorRole = "Roles:Sponsor";
        public const string LeadRole = "Roles:Lead";
        public const string MemberRole = "Roles:Member";
        public const string CapstoneOrg = "SupportedOrgs:Capstone";

        public static void TestKey(object key)
        {
            if (key == null) {
                throw new ArgumentNullException($"Configuration value {key} was not found."); 
            }
        }

        public static string FeedbackDbConnectionString(string env)
        {     
            return ConnectionStringPrefix + env + ":Feedback";
        }

        public static string RolesDbConnectionString(string env)
        {
            return ConnectionStringPrefix + env + ":System";
        }
    }
}
