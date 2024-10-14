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

        public async Task<IEnumerable<FeedbackPdf>> GetFeedbackByProjectIdAndRoundId(Guid projectId, int roundId)
        {
            _logger.LogInformation("Getting feedback by project id, time frame id, and round id...");
            return await _dbSet
                .Include(x => x.Round)
                .Include(x => x.Project)
                .Include(x => x.User)
                .Where(f => f.ProjectId == projectId && f.RoundId == roundId)
                .ToListAsync();
        }
    }
}
