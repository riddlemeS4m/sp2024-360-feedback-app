using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Services.FeedbackDb
{
    public class MetricResponseService : GenericFeedbackDbService<MetricResponse>
    {
        private readonly ILogger<MetricResponseService> _logger;
        public MetricResponseService(IFeedbackDbContext feedbackDb, ILogger<MetricResponseService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }
    }
}
