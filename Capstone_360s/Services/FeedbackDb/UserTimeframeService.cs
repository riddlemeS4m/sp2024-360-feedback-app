using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class UserTimeframeService : GenericFeedbackDbService<UserTimeframe>
    {
        private readonly ILogger<UserTimeframeService> _logger;
        public UserTimeframeService(IFeedbackDbContext feedbackDb, 
            ILogger<UserTimeframeService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<UserTimeframe>> GetUserTimeframesByUserId(string userId)
        {
            _logger.LogInformation("Getting user timeframes by user id...");

            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.Timeframe)
                .Where(x => x.UserId == Guid.Parse(userId))
                .ToListAsync();
        }

        public async Task<IEnumerable<UserTimeframe>> GetUsersByTimeframeId(int timeframeId)
        {
            _logger.LogInformation("Getting users by timeframe id...");

            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.Timeframe)
                .Where(x => x.TimeframeId == timeframeId)
                .Distinct()
                .ToListAsync();
        }
    }
}