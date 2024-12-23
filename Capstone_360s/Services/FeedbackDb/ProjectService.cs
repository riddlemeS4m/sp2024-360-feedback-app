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
            var projects = await _dbSet.Include(x => x.Timeframe)
                .Include(x => x.POC)
                .Include(x => x.Manager)
                .Where(p => p.OrganizationId == organizationIdToGuid && p.TimeframeId == timeframeId)
                .ToListAsync();
            return projects;
        }

        public async Task<Dictionary<string, Guid>> GetProjectsDictionaryByTimeframeId(string organizationId, int timeframeId)
        {
            _logger.LogInformation("Getting projects dictionary by organization id and timeframe id...");

            var projects = await GetProjectsByTimeframeId(organizationId, timeframeId);
            var projectsDictionary = projects.ToDictionary(p => p.Name, p => p.Id);
            return projectsDictionary;
        }

        public async Task<IEnumerable<Project>> GetProjectsByIds(List<Guid> ids)
        {
            _logger.LogInformation("Getting projects by ids...");

            return await _dbSet.Include(x => x.POC).Include(x => x.Manager).Where(x => ids.Contains(x.Id)).ToListAsync();
        }
    }
}
