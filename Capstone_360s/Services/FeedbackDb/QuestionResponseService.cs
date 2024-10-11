using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Services.FeedbackDb
{
    public class QuestionResponseService : GenericFeedbackDbService<QuestionResponse>
    {
        private readonly ILogger<QuestionResponseService> _logger;
        public QuestionResponseService(IFeedbackDbContext feedbackDb, ILogger<QuestionResponseService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }
    }
}
