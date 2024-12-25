namespace Capstone_360s.Interfaces.IService
{
    public interface IConfigureEnvironment
    {
        string SendGridKey { get; }
        string MicrosoftClientId { get; }
        string MicrosoftTenantId { get; }
        string MicrosoftClientSecret { get; }
        string MicrosoftDomain { get; }
        string MicrosoftTokenAcquisitionScopes { get; }
        string MicrosoftGraphScopes { get; }
        string MicrosoftInstance { get; }
        string MicrosoftGraphBaseUrl { get; }
        string MicrosoftCallbackPath { get; }
        string GoogleCredentials { get; }
        string FeedbackDbConnection { get; }
        string RolesDbConnection { get; }
        string SystemAdministrator { get; }
        string ProgramManager { get; }
        string Instructor { get; }
        string TeamLead { get; }
        string Member { get; }
        string CapstoneOrg { get; }
    }
}