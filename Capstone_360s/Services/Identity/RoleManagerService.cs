using Capstone_360s.Interfaces.IService;
using MySqlConnector;
using System.Data;

namespace Capstone_360s.Services.Identity
{
    public class RoleManagerService : IRoleManager
    {
        public const string AdminOnlyPolicy = "AdministratorOnly";
        public const string SponsorOnlyPolicy = "SponsorOnly";
        public const string LeadOnlyPolicy = "LeadOnly";
        public const string MemberOnlyPolicy = "MemberOnly";
        private const int CommandTimeoutSeconds = 30;                // Command execution timeout
        private const int MaxRetries = 3;                            // Number of retry attempts

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

            int attempt = 0;

            while (attempt < MaxRetries)
            {
                attempt++;
                await using var connection = new MySqlConnection(_connectionString);
                try
                {
                    _logger.LogInformation("Opening connection...");
                    await connection.OpenAsync();

                    await using var command = new MySqlCommand
                    {
                        Connection = connection,
                        CommandText = @"
                            SELECT roleName 
                            FROM user_roles 
                            JOIN roles ON user_roles.roleID = roles.roleID 
                            JOIN apps ON roles.appID = apps.appID 
                            WHERE appName = '360Feedback' 
                            AND userID = @UserID;",
                        CommandTimeout = CommandTimeoutSeconds  // Timeout for the query
                    };
                    
                    _logger.LogInformation("Adding parameters...");
                    command.Parameters.AddWithValue("@UserID", microsoftId);

                    _logger.LogInformation("Executing reader...");
                    await using var reader = await command.ExecuteReaderAsync();
                    var roles = new List<string>();

                    _logger.LogInformation("Reading...");
                    while (await reader.ReadAsync())
                    {
                        roles.Add(reader.GetString(0));
                    }

                    _logger.LogInformation("Disposing connection...");
                    await connection.DisposeAsync();

                    return roles;
                }
                catch (MySqlException ex) when (ex.Number == 1042 || ex.Number == 1045 || ex.Number == 0)
                {
                    // Retry on connection or timeout errors
                    _logger.LogWarning("Database connection failed. Attempt {0} of {1}. Error: {2}", attempt, MaxRetries, ex.Message);

                    if (connection.State == ConnectionState.Open)
                    {
                        _logger.LogInformation("Disposing connection...");
                        await connection.DisposeAsync();
                    }

                    if (attempt >= MaxRetries)
                    {
                        _logger.LogError("Maximum retry attempts reached. Failing request.");
                        throw;  // Re-throw after final failure
                    }
                }
            }

            return Enumerable.Empty<string>();  // Fallback if retries exhausted
        }
    }
}
