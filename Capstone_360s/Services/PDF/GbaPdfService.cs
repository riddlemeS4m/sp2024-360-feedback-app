using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.Organizations.GBA;
using Capstone_360s.Services.FeedbackDb;
using Capstone_360s.Services.Maps;
using Capstone_360s.Utilities;

namespace Capstone_360s.Services.PDF
{
    [Organization("Gba")]
    public class GbaPdfService : IWritePdf<GbaDocument, GbaInvertedSurvey>
    {
        private readonly FeedbackDbServiceFactory _serviceFactory;
        private readonly GbaMapToInvertedSurvey _invertSurveyService;
        private readonly ILogger<GbaPdfService> _logger;

        public GbaPdfService(FeedbackDbServiceFactory serviceFactory,
            GbaMapToInvertedSurvey invertSurveyService,
            ILogger<GbaPdfService> logger) 
        {
            _serviceFactory = serviceFactory;
            _invertSurveyService = invertSurveyService;
            _logger = logger;
        }

        public Task<List<FeedbackPdf>> GeneratePdfs(IEnumerable<GbaInvertedSurvey> invertedQualtrics, int currentRoundId)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> WritePdfAsync(IWritePdf<GbaDocument, GbaInvertedSurvey>.WritePdfContent<GbaDocument> pdfWriter, GbaDocument documentMaterial)
        {
            throw new NotImplementedException();
        }
    }
}
