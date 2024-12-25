using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class TimeframeService : GenericFeedbackDbService<Timeframe>
    {
        private readonly ILogger<TimeframeService> _logger;
        public TimeframeService(IFeedbackDbContext feedbackDb, ILogger<TimeframeService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Timeframe>> GetTimeframesByOrganizationId(string id)
        {
            _logger.LogInformation("Getting timeframes by organization id...");

            var idToGuid = Guid.Parse(id);
            var timeframes = await _dbSet.Where(t => t.OrganizationId == idToGuid).ToListAsync();
            return timeframes;
        }

        public async Task<IEnumerable<Timeframe>> GetTimeframesByIds(List<int> ids)
        {
            _logger.LogInformation("Getting timeframes by list of ids...");

            return await _dbSet.Where(x => ids.Contains(x.Id) && !x.IsArchived).ToListAsync();
        }
    }
}
