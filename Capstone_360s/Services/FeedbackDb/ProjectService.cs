using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class ProjectService : GenericFeedbackDbService<Project>
    {
        private readonly ILogger<ProjectService> _logger;
        public ProjectService(IFeedbackDbContext feedbackDb, ILogger<ProjectService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Project>> GetProjectsByTimeframeId(string organizationId, int timeframeId)
        {
            _logger.LogInformation("Getting projects by organization id and timeframe id...");

            var organizationIdToGuid = Guid.Parse(organizationId);
            var projects = await _dbSet.Where(p => p.OrganizationId == organizationIdToGuid && p.TimeframeId == timeframeId).ToListAsync();
            return projects;
        }
    }
}
