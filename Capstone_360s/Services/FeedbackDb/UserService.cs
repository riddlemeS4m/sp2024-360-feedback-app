using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Services.FeedbackDb
{
    public class UserService : GenericFeedbackDbService<User>
    {
        private readonly ILogger<UserService> _logger;
        public UserService(IFeedbackDbContext feedbackDb, ILogger<UserService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }
    }
}
