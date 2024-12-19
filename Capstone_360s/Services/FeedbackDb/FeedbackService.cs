using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class FeedbackService: GenericFeedbackDbService<Feedback>
    {
        private readonly ILogger<FeedbackService> _logger;
        public FeedbackService(IFeedbackDbContext feedbackDb, 
            ILogger<FeedbackService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Feedback>> GetFeedbackByTimeframeIdAndRoundId(int timeframeId, int roundId)
        {
            _logger.LogInformation("Getting feedback by project id, time frame id, and round id...");
            return await _dbSet.Include(x => x.Timeframe)
                .Include(x => x.Round)
                .Include(x => x.Project)
                .Include(x => x.Reviewer)
                .Where(f => f.TimeframeId == timeframeId && f.RoundId == roundId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetMultipleRoundsOfFeedbackByTimeframeIdAndRoundId(int timeframeId, int roundId)
        {
            _logger.LogInformation("Getting feedback by project id, time frame id, and round id...");
            return await _dbSet.Include(x => x.Timeframe)
                .Include(x => x.Round)
                .Include(x => x.Project)
                .Include(x => x.Reviewer)
                .Where(f => f.TimeframeId == timeframeId && f.RoundId <= roundId)
                .GroupBy(f => new { f.RevieweeId, f.ReviewerId, f.ProjectId, f.TimeframeId, f.RoundId, f.OriginalResponseId })
                .Select(g => g.First())
                .ToListAsync();
        }
    }
}
