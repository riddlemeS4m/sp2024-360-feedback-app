using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class MetricService : GenericFeedbackDbService<Metric>
    {
        private readonly ILogger<MetricService> _logger;
        public MetricService(IFeedbackDbContext feedbackDb, ILogger<MetricService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Metric>> GetMetricsByOrganizationId(Guid organizationId)
        {
            _logger.LogInformation($"Getting metrics for timeframe {organizationId}...");

            var metrics = await _dbSet.Where(m => m.OrganizationId == organizationId).ToListAsync();
            return metrics;
        }

        public async Task<IEnumerable<Metric>> GetDefaultCapstoneMetrics(Guid organizationId, List<string> originalMetricIds)
        {
            _logger.LogInformation($"Getting default capstone metrics...");

            var metrics = await _dbSet.Where(m => originalMetricIds.Contains(m.OriginalMetricId) && m.OrganizationId == organizationId).ToListAsync();
            return metrics;
        }
    }
}
