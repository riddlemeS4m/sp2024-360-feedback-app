using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class UserService : GenericFeedbackDbService<User>
    {
        private readonly ILogger<UserService> _logger;
        public UserService(IFeedbackDbContext feedbackDb, ILogger<UserService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetUsersByOrganizationId(Guid organizationId)
        {
            return await _dbSet.Where(x => x.OrganizationId == organizationId).ToListAsync();
        }
    }
}
