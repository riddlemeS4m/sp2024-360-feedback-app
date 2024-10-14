using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Services.FeedbackDb
{
    public class OrganizationService: GenericFeedbackDbService<Organization>
    {
        private readonly ILogger<OrganizationService> _logger;
        public OrganizationService(IFeedbackDbContext feedbackDb, ILogger<OrganizationService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }


    }
}
