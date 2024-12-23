using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Interfaces.IService
{
    public interface IRoleManager
    {
        public Task<IEnumerable<string>> GetRoles(Guid microsoftId);
        public Task<IEnumerable<User>> GetUsersByRole(string role);
        public Task<User> AddNewUser(string email);
        public Task AddUserToRole(string id, string role);
    }
}