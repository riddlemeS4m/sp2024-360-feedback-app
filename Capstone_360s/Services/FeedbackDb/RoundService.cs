using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class RoundService : GenericFeedbackDbService<Round>
    {
        private readonly ILogger<RoundService> _logger;
        public RoundService(IFeedbackDbContext feedbackDb, ILogger<RoundService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Round>> GetFirstNRounds(int n)
        {
            _logger.LogInformation($"Getting first {n} rounds...");

            var rounds = await _dbSet.OrderBy(x => x.Id).Take(n).ToListAsync();
            return rounds;
        }
    }
}
