using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Services.FeedbackDb
{
    public class QuestionService : GenericFeedbackDbService<Question>
    {
        private readonly ILogger<QuestionService> _logger;
        public QuestionService(IFeedbackDbContext feedbackDb, ILogger<QuestionService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }
    }
}
