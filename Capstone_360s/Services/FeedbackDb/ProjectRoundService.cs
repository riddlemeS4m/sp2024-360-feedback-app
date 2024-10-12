using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class ProjectRoundService : GenericFeedbackDbService<ProjectRound>
    {
        private readonly ILogger<ProjectRoundService> _logger;
        public ProjectRoundService(IFeedbackDbContext feedbackDb, ILogger<ProjectRoundService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<ProjectRound>> GetProjectRoundsByProjectId(string projectId)
        {
            _logger.LogInformation("Getting project rounds by project id...");

            var projectIdToGuid = Guid.Parse(projectId);
            var projectRounds = await _dbSet.Include(x => x.Round).Include(x => x.Project).Where(pr => pr.ProjectId == projectIdToGuid).ToListAsync();
            return projectRounds;
        }
    }
}
