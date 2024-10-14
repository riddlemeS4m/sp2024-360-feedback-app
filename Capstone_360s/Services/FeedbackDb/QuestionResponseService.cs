using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class QuestionResponseService : GenericFeedbackDbService<QuestionResponse>
    {
        private readonly ILogger<QuestionResponseService> _logger;
        public QuestionResponseService(IFeedbackDbContext feedbackDb, ILogger<QuestionResponseService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<Dictionary<Guid, IEnumerable<QuestionResponse>>> GetQuestionResponsesDictByFeedbackIds(List<Guid> questionIds)
        {
            ArgumentNullException.ThrowIfNull(questionIds);

            var dict = new Dictionary<Guid, IEnumerable<QuestionResponse>>();
            questionIds.Sort((x, y) => x.CompareTo(y));

            var questionResponses = await _dbSet.Include(x => x.Question).Include(x => x.Feedback).Where(mr => questionIds.Contains(mr.FeedbackId)).ToListAsync();
            questionResponses.Sort((x, y) => x.FeedbackId.CompareTo(y.FeedbackId));

            foreach (var key in questionIds)
            {
                dict.Add(key, questionResponses.Where(x => x.FeedbackId == key));
            }

            if (questionIds.Count != dict.Count)
            {
                throw new Exception("Not all feedback objects have questionresponses associated with them.");
            }

            return dict;
        }

        public async Task<IEnumerable<QuestionResponse>> GetQuestionResponsesByFeedbackId(Guid feedbackId)
        {
            return await _dbSet.Where(qr => qr.FeedbackId == feedbackId)
                .ToListAsync();
        }
    }
}
