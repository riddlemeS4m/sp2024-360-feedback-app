using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.Organizations.Capstone;
using Capstone_360s.Utilities;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Capstone_360s.Services.CSV
{
    [Organization("Capstone")]
    public class CapstoneCsvService : IAccessCsvFile<Qualtrics>
    {
        private readonly ClassMap<Qualtrics> _classMap;
        private readonly ILogger<CapstoneCsvService> _logger;
        public CapstoneCsvService(ClassMap<Qualtrics> classMap, 
            ILogger<CapstoneCsvService> logger)
        {
            _classMap = classMap;
            _logger = logger;
        }

        public IEnumerable<Qualtrics> ReadSurveyResponses(IFormFile file, IEnumerable<string> expectedHeaders)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Qualtrics> ReadSurveyResponsesWithFilterDate(IFormFile file, IEnumerable<string> expectedHeaders, string dateField, DateTime filterDate,
            IAccessCsvFile<Qualtrics>.FilterDateFunc filterDateFunc)
        {
            // Check if the file is null or empty
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("CSV file is required and cannot be empty.");
            }

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Read the header row
            csv.Read();
            csv.ReadHeader();

            var headers = csv.HeaderRecord;

            // Verify that the headers match
            var missingHeaders = expectedHeaders.Except(headers).ToList();
            if (missingHeaders.Count != 0)
            {
                throw new Exception($"The following expected headers are missing: {string.Join(", ", missingHeaders)}");
            }

            // Register the class map and read the records
            csv.Context.RegisterClassMap(_classMap);

            // Manually read records and filter rows before mapping
            var validRecords = new List<Qualtrics>();
            while (csv.Read())
            {
                // Use the custom date filter logic (abstracted via delegate)
                string startDateField = csv.GetField(dateField); // Assuming StartDate field is in the CSV
                if (filterDateFunc(startDateField, filterDate))
                {
                    // Map the record if it passes the filter
                    var record = csv.GetRecord<Qualtrics>();
                    validRecords.Add(record);
                }
            }

            return validRecords;
        }
    }
}
