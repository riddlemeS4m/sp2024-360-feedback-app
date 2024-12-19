using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.Organizations.GBA;
using Capstone_360s.Utilities;
using CsvHelper.Configuration;

namespace Capstone_360s.Services.CSV
{
    [Organization("Gba")]
    public class GbaCsvService : IAccessCsvFile<GbaSurvey>
    {
        private readonly ClassMap<GbaSurvey> _classMap;
        private readonly ILogger<GbaCsvService> _logger;
        public GbaCsvService(ClassMap<GbaSurvey> classMap, 
            ILogger<GbaCsvService> logger) 
        {
            _classMap = classMap;
            _logger = logger;
        }

        public IEnumerable<GbaSurvey> ReadSurveyResponses(IFormFile file, IEnumerable<string> expectedHeaders)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<GbaSurvey> ReadSurveyResponsesWithFilterDate(IFormFile file, IEnumerable<string> expectedHeaders, string dateField, DateTime filterDate, 
            IAccessCsvFile<GbaSurvey>.FilterDateFunc filterDateFunc)
        {
            throw new NotImplementedException();
        }
    }
}
