using Dapper;
using MySql.Data.MySqlClient;

namespace Capstone_360s.Services.Identity
{
    public class RoleManagerService
    {
        public const string AdminOnlyPolicy = "AdministratorOnly";
        public const string SponsorOnlyPolicy = "SponsorOnly";
        public const string LeadOnlyPolicy = "LeadOnly";
        public const string MemberOnlyPolicy = "MemberOnly";

        private readonly string _connectionString;
        private readonly ILogger<RoleManagerService> _logger;

        public RoleManagerService(string connectionString, ILogger<RoleManagerService> logger) 
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> GetRoles(Guid microsoftId)
        {
            _logger.LogInformation("Getting roles for user {0}", microsoftId);

            using var connection = new MySqlConnection(_connectionString);
            var query = "select roleName from user_roles join roles on user_roles.roleID = roles.roleID join apps on roles.appID = apps.appID where appName like '360Feedback' and userID = @UserID;";
            return await connection.QueryAsync<string>(query, new { UserId = microsoftId});
        }
    }
}
