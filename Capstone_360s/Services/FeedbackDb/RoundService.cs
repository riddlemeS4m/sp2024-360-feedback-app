using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Services.FeedbackDb
{
    public class RoundService : GenericFeedbackDbService<Round>
    {
        private readonly ILogger<RoundService> _logger;
        public RoundService(IFeedbackDbContext feedbackDb, ILogger<RoundService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }
    }
}
