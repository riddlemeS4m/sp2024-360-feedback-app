using Capstone_360s.Interfaces.IService;
using MySql.Data.MySqlClient;

namespace Capstone_360s.Services.Identity
{
    public class RoleManagerService : IRoleManager
    {
        public const string AdminOnlyPolicy = "AdministratorOnly";
        public const string SponsorOnlyPolicy = "SponsorOnly";
        public const string LeadOnlyPolicy = "LeadOnly";
        public const string MemberOnlyPolicy = "MemberOnly";

        private static readonly SemaphoreSlim _semaphore = new(10);  // Limit concurrent DB calls
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

            await _semaphore.WaitAsync();

            int attempt = 0;

            try
            {
                while (attempt < MaxRetries)
                {
                    attempt++;
                    await using var connection = new MySqlConnection(_connectionString);
                    try
                    {
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
                        
                        command.Parameters.AddWithValue("@UserID", microsoftId);

                        using var reader = await command.ExecuteReaderAsync();
                        var roles = new List<string>();

                        while (await reader.ReadAsync())
                        {
                            roles.Add(reader.GetString(0));
                        }

                        await connection.CloseAsync();
                        _semaphore.Release();

                        return roles;
                    }
                    catch (MySqlException ex) when (ex.Number == 1042 || ex.Number == 1045 || ex.Number == 0)
                    {
                        // Retry on connection or timeout errors
                        _logger.LogWarning("Database connection failed. Attempt {0} of {1}. Error: {2}", attempt, MaxRetries, ex.Message);

                        if (attempt >= MaxRetries)
                        {
                            _logger.LogError("Maximum retry attempts reached. Failing request.");
                            throw;  // Re-throw after final failure
                        }

                        await Task.Delay(TimeSpan.FromSeconds(attempt * 2));  // Exponential backoff
                    }
                }

                return Enumerable.Empty<string>();  // Fallback if retries exhausted
            }
            finally
            {
                _semaphore.Release();  // Ensure semaphore is released even if the method throws
            }
        }
    }
}
