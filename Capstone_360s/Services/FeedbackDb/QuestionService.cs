using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class QuestionService : GenericFeedbackDbService<Question>
    {
        private readonly ILogger<QuestionService> _logger;
        public QuestionService(IFeedbackDbContext feedbackDb, ILogger<QuestionService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Question>> GetQuestionsByOrganizationId(Guid organizationId)
        {
            _logger.LogInformation("Getting questions for round {organizationId}...", organizationId);

            var questions = await _dbSet.Where(q => q.OrganizationId == organizationId).ToListAsync();
            return questions;
        }

        public async Task<IEnumerable<Question>> GetDefaultCapstoneQuestions(Guid organizationId, List<string> originalQuestionIds)
        {
            _logger.LogInformation($"Getting default capstone questions...");

            var questions = await _dbSet.Where(q => originalQuestionIds.Contains(q.OriginalQuestionId) && q.OrganizationId == organizationId).ToListAsync();
            return questions;
        }
    }
}
