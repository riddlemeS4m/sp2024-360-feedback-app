using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Services.FeedbackDb
{
    public class ProjectRoundService : GenericFeedbackDbService<ProjectRound>
    {
        private readonly ILogger<ProjectRoundService> _logger;
        public ProjectRoundService(IFeedbackDbContext feedbackDb, ILogger<ProjectRoundService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }
    }
}
