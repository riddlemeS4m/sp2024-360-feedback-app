using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Interfaces.IService
{
    public interface IRoleManager
    {
        public Task<IEnumerable<string>> GetRoles(Guid microsoftId);
        public Task<IEnumerable<User>> GetUsersByRole(string organizationId, string role);
        public Task<User> AddNewUser(string organizationId, string email);
        public Task AddUserToRole(string organizationId, string userId, string role);
    }
}