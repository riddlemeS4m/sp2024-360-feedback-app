using Microsoft.Graph;

namespace Capstone_360s.Interfaces.IService
{
    public interface IMicrosoftGraph
    {
        public Task<User> GetUserIdByEmailAsync(string email);
    }
}