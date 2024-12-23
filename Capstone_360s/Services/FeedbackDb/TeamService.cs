using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class TeamService : GenericFeedbackDbService<TeamMember>
    {
        private readonly ILogger<TeamService> _logger;
        public TeamService(IFeedbackDbContext feedbackDb, ILogger<TeamService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<TeamMember>> GetTeamMembersByProjectId(Guid projectId)
        {
            _logger.LogInformation("Getting team members by project id...");

            var teamMembers = await _dbSet.Include(x => x.Project).Include(x => x.User).Where(p => p.ProjectId == projectId).ToListAsync();
            return teamMembers;
        }

        public async Task<IEnumerable<TeamMember>> GetTeamMembersByUserId(Guid userId)
        {
            _logger.LogInformation("Getting team members by user id...");

            var teamMembers = await _dbSet.Include(x => x.Project).Include(x => x.User).Where(p => p.UserId == userId).ToListAsync();
            return teamMembers;
        }

        public async Task<IEnumerable<TeamMember>> GetTeamMembersByProjectIdAndUserId(Guid projectId, Guid userId)
        {
            _logger.LogInformation("Getting team members by project id and user id...");

            var teamMembers = await _dbSet.Include(x => x.Project).Include(x => x.User).Where(p => p.ProjectId == projectId && p.UserId == userId).ToListAsync();
            return teamMembers;
        }

        public async Task<IEnumerable<TeamMember>> GetTeamMembersByProjectIdAndUserEmail(Guid projectId, string userEmail)
        {
            _logger.LogInformation("Getting team members by project id and user email...");

            var teamMembers = await _dbSet.Include(x => x.Project).Include(x => x.User).Where(p => p.ProjectId == projectId && p.User.Email == userEmail).ToListAsync();
            return teamMembers;
        }

        public async Task<IEnumerable<TeamMember>> GetTeamMembersByListOfProjectIds(List<Guid> projectIds)
        {
            _logger.LogInformation("Getting team members by list of project ids...");

            var teamMembers = await _dbSet.Include(x => x.Project)
                .Include(x => x.User)
                .Where(p => projectIds.Contains(p.ProjectId))
                .ToListAsync();
            return teamMembers;
        }

        public async Task<IEnumerable<int>> GetTimeframeIdsByTeamMember(string userId, string orgId)
        {
            _logger.LogInformation("Getting timeframe ids by user id and organization id...");

            var userGuid = Guid.Parse(userId);
            var orgGuid = Guid.Parse(orgId);

            return await _dbSet
                .Join(_context.Projects,
                    t => t.ProjectId,
                    p => p.Id,
                    (t, p) => new { t, p })
                .Where(tp => (tp.t.UserId == userGuid 
                        || tp.p.POCId == userGuid
                        || tp.p.ManagerId == userGuid)
                        && tp.p.OrganizationId == orgGuid)
                .Select(tp => tp.p.TimeframeId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<Guid>> GetProjectIdsByTeamMember(string userId, int timeframeId, string orgId)
        {
            _logger.LogInformation("Getting timeframe ids by user id and organization id...");

            var userGuid = Guid.Parse(userId);
            var orgGuid = Guid.Parse(orgId);

            return await _dbSet
                .Join(_context.Projects,
                    t => t.ProjectId,
                    p => p.Id,
                    (t, p) => new { t, p })
                .Where(tp => (tp.t.UserId == userGuid 
                        || tp.p.POCId == userGuid
                        || tp.p.ManagerId == userGuid)
                        && tp.p.TimeframeId == timeframeId
                        && tp.p.OrganizationId == orgGuid)
                .Select(tp => tp.p.Id)
                .Distinct()
                .ToListAsync();
        }
    }
}
