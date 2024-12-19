using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class FeedbackPdfService : GenericFeedbackDbService<FeedbackPdf>
    {
        private readonly ILogger<FeedbackPdfService> _logger;
        public FeedbackPdfService(IFeedbackDbContext feedbackDb, 
                       ILogger<FeedbackPdfService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="roundId"></param>
        /// <returns>An enumerable list of FeedbackPdfs, including Rounds, Projects, and Users</returns>
        public async Task<IEnumerable<FeedbackPdf>> GetFeedbackByProjectIdAndRoundId(Guid projectId, int roundId)
        {
            _logger.LogInformation("Getting feedback pdfs by project id and round id...");
            return await _dbSet
                .Include(x => x.Round)
                .Include(x => x.Project)
                .Include(x => x.User)
                .Where(f => f.ProjectId == projectId && f.RoundId == roundId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeedbackPdf>> GetFeedbackPdfsByProjectIdsAndRoundId(List<Guid> projectIds, int roundId)
        {
            _logger.LogInformation("Getting feedback pdfs for multiple projects and round id");

            return await _dbSet
                .Where(f => projectIds.Contains(f.ProjectId) && f.RoundId == roundId)
                .ToListAsync();
        }
    }
}
