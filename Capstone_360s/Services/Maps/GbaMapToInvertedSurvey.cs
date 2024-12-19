using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.Organizations.GBA;
using Capstone_360s.Services.FeedbackDb;
using Capstone_360s.Utilities;

namespace Capstone_360s.Services.Maps
{
    [Organization("Gba")]
    public class GbaMapToInvertedSurvey : IMapFeedback<GbaInvertedSurvey>
    {
        private readonly FeedbackDbServiceFactory _serviceFactory;
        private readonly ILogger<GbaMapToInvertedSurvey> _logger;
        public GbaMapToInvertedSurvey(FeedbackDbServiceFactory serviceFactory,
            ILogger<GbaMapToInvertedSurvey> logger) 
        { 
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        public Task<IEnumerable<GbaInvertedSurvey>> MapFeedback(IEnumerable<Feedback> feedback)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<GbaInvertedSurvey>> MapFeedback(IEnumerable<Feedback> feedback, int noOfRounds)
        {
            throw new NotImplementedException();
        }
    }
}
