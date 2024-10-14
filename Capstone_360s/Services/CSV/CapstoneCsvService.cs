using Capstone_360s.Models.CapstoneRoster;
using CsvHelper.Configuration;

namespace Capstone_360s.Services.CSV
{
    public class CapstoneCsvService : GenericCsvService<Qualtrics>
    {
        private readonly ILogger<CapstoneCsvService> _logger;
        public CapstoneCsvService(ClassMap<Qualtrics> classMap, ILogger<CapstoneCsvService> logger)
            : base(classMap, logger)
        {
            _logger = logger;
        }
    }
}
