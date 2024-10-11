using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Services.FeedbackDb
{
    public class TeamService : GenericFeedbackDbService<Team>
    {
        private readonly ILogger<TeamService> _logger;
        public TeamService(IFeedbackDbContext feedbackDb, ILogger<TeamService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }
    }
}
