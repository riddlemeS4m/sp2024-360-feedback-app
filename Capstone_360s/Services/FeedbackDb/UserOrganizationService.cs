using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class UserOrganizationService : GenericFeedbackDbService<UserOrganization>
    {
        private readonly ILogger<UserOrganizationService> _logger;
        public UserOrganizationService(IFeedbackDbContext feedbackDb, ILogger<UserOrganizationService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<UserOrganization>> GetUsersByOrganizationId(Guid organizationId)
        {
            return await _dbSet
                .Include(x => x.User)
                .Where(x => x.OrganizationId == organizationId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<UserOrganization>> GetOrganizationsByUserId(Guid userId)
        {
            return await _dbSet.Include(x => x.Organization).Where(x => x.UserId == userId).ToListAsync();
        }
    }
}
