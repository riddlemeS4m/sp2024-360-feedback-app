﻿using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class FeedbackPdfService : GenericFeedbackDbService<FeedbackPdf>
    {
        private readonly ILogger<FeedbackPdfService> _logger;
        public FeedbackPdfService(IFeedbackDbContext feedbackDb, 
                       ILogger<FeedbackPdfService> logger) : base(feedbackDb)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="roundId"></param>
        /// <returns>An enumerable list of FeedbackPdfs, including Rounds, Projects, and Users</returns>
        public async Task<IEnumerable<FeedbackPdf>> GetFeedbackByProjectIdAndRoundId(string organizationId, int timeframeId, string projectId, int roundId)
        {
            _logger.LogInformation("Getting feedback pdfs by project id and round id...");

            var orgGuid = Guid.Parse(organizationId);
            var projectGuid = Guid.Parse(projectId);

            return await _dbSet
                .Include(x => x.Round)
                .Include(x => x.Project)
                .Include(x => x.User)
                .Where(f => f.ProjectId == projectGuid 
                    && f.RoundId == roundId 
                    && f.Project.TimeframeId == timeframeId 
                    && f.Project.OrganizationId == orgGuid)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeedbackPdf>> GetFeedbackPdfsByProjectIdsAndRoundId(List<Guid> projectIds, int roundId)
        {
            _logger.LogInformation("Getting feedback pdfs for multiple projects and round id");

            return await _dbSet
                .Where(f => projectIds.Contains(f.ProjectId) && f.RoundId == roundId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeedbackPdf>> GetFeedbackPdfsByIds(List<Guid> ids)
        {
            _logger.LogInformation("Getting feedback pdfs by list of ids...");

            return await _dbSet.Include(x => x.Round)
                .Include(x => x.Project)
                .Include(x => x.User)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<FeedbackPdf>> GetFeedbackPdfsByUserId(string organizationId, int timeframeId, string projectId, int roundId, string userId)
        {
            _logger.LogInformation("Getting feedback pdfs by user id...");

            return await _dbSet.Include(x => x.Round)
                .Include(x => x.Project)
                .ThenInclude(x => x.Timeframe)
                .Include(x => x.User)
                .Where(f => f.UserId == Guid.Parse(userId)
                    && f.ProjectId == Guid.Parse(projectId) 
                    && f.RoundId == roundId 
                    && f.Project.TimeframeId == timeframeId 
                    && f.Project.OrganizationId == Guid.Parse(organizationId)
                    && !f.Project.Timeframe.IsArchived)
                .ToListAsync();
        }
    }
}
