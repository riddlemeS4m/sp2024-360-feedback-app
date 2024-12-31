using Capstone_360s.Interfaces;
using Capstone_360s.Interfaces.IService;
using Microsoft.Graph;
using MySqlConnector;
using System.Data;

namespace Capstone_360s.Services.Identity
{
    public class RoleManagerService : IRoleManager
    {
        public const string SystemAdministratorOnlyPolicy = "SystemAdministratorOnly";
        public const string ProgramManagerOnlyPolicy = "ProgramManagerOnly";
        public const string InstructorOnlyPolicy = "InstructorOnly";
        public const string TeamLeadOnlyPolicy = "TeamLeadOnly";
        public const string MemberOnlyPolicy = "MemberOnly";
        private const int CommandTimeoutSeconds = 5;
        private const int MaxRetries = 3;

        private readonly string _connectionString;
        private readonly IMicrosoftGraph _microsoftGraphClient;
        private readonly IFeedbackDbServiceFactory _dbServiceFactory;
        private readonly IConfigureEnvironment _config;
        private readonly ILogger<RoleManagerService> _logger;

        public RoleManagerService(IConfigureEnvironment config, 
            IMicrosoftGraph microsoftGraph,
            IFeedbackDbServiceFactory dbServiceFactory,
            ILogger<RoleManagerService> logger) 
        {
            _connectionString = config.RolesDbConnection;
            _microsoftGraphClient = microsoftGraph;
            _dbServiceFactory = dbServiceFactory;
            _config = config;
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

                    if(!roles.Any())
                    {
                        _logger.LogInformation("User does not have any defined roles.");
                    }

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

            return new List<string>();  // Fallback if retries exhausted
        }

        public async Task<IEnumerable<Capstone_360s.Models.FeedbackDb.User>> GetUsersByRole(string organizationId, string role)
        {
            _logger.LogInformation("Getting users by role...");

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
                            SELECT users.userId, users.userEmail, users.userFullName
                            FROM user_roles 
                            JOIN users ON user_roles.userid = users.userid
                            JOIN roles ON user_roles.roleid = roles.roleid
                            WHERE rolename LIKE @role;",
                        CommandTimeout = CommandTimeoutSeconds  // Timeout for the query
                    };

                    _logger.LogInformation("Adding parameters...");
                    command.Parameters.AddWithValue("@role", role);

                    _logger.LogInformation("Executing reader...");
                    await using var reader = await command.ExecuteReaderAsync();
                    var users = new List<Capstone_360s.Models.FeedbackDb.User>();

                    _logger.LogInformation("Reading...");
                    while (await reader.ReadAsync())
                    {
                        users.Add(new Capstone_360s.Models.FeedbackDb.User {
                            Id = Guid.Parse(reader["userId"].ToString()),
                            Email = reader["userEmail"].ToString(),
                            FirstName = reader["userFullName"].ToString().Split(" ")[0],
                            LastName = reader["userFullName"].ToString().Split(" ")[1]
                        });
                    }

                    _logger.LogInformation("Disposing connection...");
                    await connection.DisposeAsync();

                    if(!users.Any())
                    {
                        _logger.LogInformation("No users found with the specified role.");
                        return new List<Capstone_360s.Models.FeedbackDb.User>();
                    }
                    else
                    {
                        var emails = new List<string>();
                        foreach(var user in users)
                        {
                            emails.Add(user.Email);
                        }

                        var usersToCheck = (await _dbServiceFactory.UserService.GetUsersByListOfEmails(emails)).ToList();

                        var orgUsers = await _dbServiceFactory.UserOrganizationService.GetUsersByOrganizationId(Guid.Parse(organizationId));

                        var returnedUsers = new List<Capstone_360s.Models.FeedbackDb.User>();
                        foreach(var user in usersToCheck)
                        {
                            if(orgUsers.Any(u => u.User.Email == user.Email))
                            {
                                returnedUsers.Add(user);
                            }
                        }

                        return returnedUsers;
                    }
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

            return new List<Capstone_360s.Models.FeedbackDb.User>();  // Fallback if retries exhausted
        }

        public async Task<Capstone_360s.Models.FeedbackDb.User> AddNewUser(string organizationId, string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException("'email' cannot be empty.");
            }

            try
            {
                var microsoftUser = await _microsoftGraphClient.GetUserIdByEmailAsync(email);

                var localUser = await _dbServiceFactory.UserService.GetUserByEmail(email);

                if (localUser.Id == Guid.Empty)
                {
                    var user = new Capstone_360s.Models.FeedbackDb.User()
                    {
                        Id = Guid.Parse(microsoftUser.Id),
                        MicrosoftId = Guid.Parse(microsoftUser.Id),
                        FirstName = microsoftUser.GivenName,
                        LastName = microsoftUser.Surname,
                        Email = email
                    };

                    user = await _dbServiceFactory.UserService.AddAsync(user);

                    var userOrgs = await _dbServiceFactory.UserOrganizationService.GetOrganizationsByUserId(user.Id);

                    if(!userOrgs.Any(x => x.OrganizationId == Guid.Parse(organizationId)))
                    {
                        await _dbServiceFactory.UserOrganizationService.AddAsync(new Models.FeedbackDb.UserOrganization {
                            UserId = user.Id,
                            OrganizationId = Guid.Parse(organizationId)
                        });
                    }
                    
                    return user;
                }
                else
                {
                    if (localUser.MicrosoftId == Guid.Empty || localUser.MicrosoftId == null)
                    {
                        localUser.MicrosoftId = Guid.Parse(microsoftUser.Id);
                        localUser = await _dbServiceFactory.UserService.UpdateAsync(localUser);

                        var userOrgs = await _dbServiceFactory.UserOrganizationService.GetOrganizationsByUserId(localUser.Id);

                        if(!userOrgs.Any(x => x.OrganizationId == Guid.Parse(organizationId)))
                        {
                            await _dbServiceFactory.UserOrganizationService.AddAsync(new Models.FeedbackDb.UserOrganization {
                                UserId = localUser.Id,
                                OrganizationId = Guid.Parse(organizationId)
                            });
                        }
                    }                    

                    return localUser;
                }   
            }
            catch (ServiceException ex)
            {
                _logger.LogWarning("Microsoft Graph ServiceException: {0}", ex.Message);
                return new Models.FeedbackDb.User();
            }
            catch (UnauthorizedAccessException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred in AddNewUser: {0}, {1}", ex.GetType(), ex.Message);
                return new Models.FeedbackDb.User();
            }
        }

        public async Task AddUserToRole(string organizationId, string userId, string role)
        {
            if(string.IsNullOrEmpty(organizationId) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                throw new ArgumentNullException("OrganizationId, UserId, and Role cannot be null or empty");
            }

            _logger.LogInformation("Getting users in role 'POC'");

            var roles = await GetRoles(Guid.Parse(userId));
            var roleExists = roles.Contains(role);

            if(roleExists)
            {
                _logger.LogInformation("User is already in role");
                return;
            }

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
                            INSERT INTO user_roles (userRoleID, userID, roleID)
                            VALUES (
                                UUID(),
                                @user, 
                                (SELECT roleID FROM roles WHERE roleName = @role)
                            );",
                        CommandTimeout = CommandTimeoutSeconds  // Timeout for the query
                    };

                    _logger.LogInformation("Adding parameters...");
                    command.Parameters.AddWithValue("@role", role);
                    command.Parameters.AddWithValue("@user", userId);

                    _logger.LogInformation("Executing writer...");
                    await command.ExecuteNonQueryAsync();

                    _logger.LogInformation("Disposing connection...");
                    await connection.DisposeAsync();

                    return;
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
        }
    }
}
