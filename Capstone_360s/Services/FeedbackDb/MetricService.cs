using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Services.FeedbackDb
{
    public class MetricService : GenericFeedbackDbService<Metric>
    {
        private readonly ILogger<MetricService> _logger;
        public MetricService(IFeedbackDbContext feedbackDb, ILogger<MetricService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }
    }
}
