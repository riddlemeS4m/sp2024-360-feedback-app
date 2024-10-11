using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;

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

    }
}
