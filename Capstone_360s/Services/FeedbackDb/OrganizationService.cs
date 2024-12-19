using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class OrganizationService: GenericFeedbackDbService<Organization>
    {
        private readonly ILogger<OrganizationService> _logger;
        public OrganizationService(IFeedbackDbContext feedbackDb, ILogger<OrganizationService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        public Organization GetByTypeAsync(string type)
        {
            return _dbSet.Where(x => x.Type == type).FirstOrDefault() ?? throw new Exception($"Organization '{type}'not found.");
        }
    }
}
