using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class MetricResponseService : GenericFeedbackDbService<MetricResponse>
    {
        private readonly ILogger<MetricResponseService> _logger;
        public MetricResponseService(IFeedbackDbContext feedbackDb, ILogger<MetricResponseService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<Dictionary<Guid, IEnumerable<MetricResponse>>> GetMetricResponsesDictByFeedbackIds(List<Guid> feedbackIds)
        {
            ArgumentNullException.ThrowIfNull(feedbackIds);

            var dict = new Dictionary<Guid, IEnumerable<MetricResponse>>();
            feedbackIds.Sort((x, y) => x.CompareTo(y));

            var metricResponses = await _dbSet.Include(x => x.Metric).Include(x => x.Feedback).Where(mr => feedbackIds.Contains(mr.FeedbackId)).ToListAsync();
            metricResponses.Sort((x, y) => x.FeedbackId.CompareTo(y.FeedbackId));

            foreach (var key in feedbackIds)
            {
                dict.Add(key, metricResponses.Where(x => x.FeedbackId == key));
            }

            if(feedbackIds.Count != dict.Count)
            {
                throw new Exception("Not all feedback objects have metricresponses associated with them.");
            }

            return dict;
        }

        public async Task<IEnumerable<MetricResponse>> GetMetricResponsesByFeedbackId(Guid feedbackId)
        {
            return await _dbSet.Where(mr => mr.FeedbackId == feedbackId)
                .ToListAsync();
        }
    }
}
