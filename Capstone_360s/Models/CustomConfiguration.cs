namespace Capstone_360s.Models
{
    public class CustomConfiguration
    {
        public const string Environment = "ASPNETCORE_ENVIRONMENT";
        public const string SendGridKey = "SendGrid";
        public const string MicrosoftClientIdKey = "Authentication:Microsoft:ClientId";
        public const string MicrosoftTenantIdKey = "Authentication:Microsoft:TenantId";
        public const string MicrosoftClientSecretKey = "Authentication:Microsoft:ClientSecret";
        public const string MicrosoftDomainKey = "Authentication:Microsoft:Domain";
        public const string MicrosoftTokenAcquisitionScopesKey = "Authentication:Microsoft:Scopes:Token";
        public const string MicrosoftGraphScopesKey = "Authentication:Microsoft:Scopes:Graph";
        public const string MicrosoftInstanceKey = "Authentication:Microsoft:Instance";
        public const string MicrosoftGraphBaseUrlKey = "Authentication:Microsoft:GraphBaseUrl";
        public const string MicrosoftCallbackPathKey = "Authentication:Microsoft:CallbackPath";
        public const string ConnectionStringKeyPrefix = "ConnectionStrings:";
        public const string GoogleDriveCredentialSection = "GoogleCredential";
        public const string SystemAdministratorRoleKey = "Roles:SystemAdministrator";
        public const string ProgramManagerRoleKey = "Roles:ProgramManager";
        public const string InstructorRoleKey = "Roles:Instructor";
        public const string TeamLeadRoleKey = "Roles:TeamLead";
        public const string MemberRoleKey = "Roles:Member";
        public const string CapstoneOrganizationFunctionalityKey = "SupportedOrgs:Capstone";

        public static void TestKey(object key)
        {
            if (key == null) {
                throw new ArgumentNullException($"Configuration value {key} was not found."); 
            }
        }

        public static string FeedbackDbConnectionString(string env)
        {     
            return ConnectionStringKeyPrefix + env + ":Feedback";
        }

        public static string RolesDbConnectionString(string env)
        {
            return ConnectionStringKeyPrefix + env + ":System";
        }
    }
}
