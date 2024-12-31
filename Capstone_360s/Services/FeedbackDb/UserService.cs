using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class UserService : GenericFeedbackDbService<User>
    {
        private readonly ILogger<UserService> _logger;
        public UserService(IFeedbackDbContext feedbackDb, ILogger<UserService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        //public async Task<IEnumerable<User>> GetUsersByOrganizationId(Guid organizationId)
        //{
        //    return await _dbSet.Where(x => x.OrganizationId == organizationId).ToListAsync();
        //}

        public async Task<IEnumerable<User>> GetUsersByListOfEmails(List<string> emails)
        {
            return await _dbSet.Where(x => emails.Contains(x.Email)).Distinct().ToListAsync();
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>User entity if user exists in db, otherwise returns an empty user with an Id of Guid.Empty</returns>
        public async Task<User> GetUserByEmail(string email)
        {
            return await _dbSet.Where(x => x.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync() ?? new User(){ Id = Guid.Empty };
        }
    }
}
