using Capstone_360s.Interfaces.IService;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Capstone_360s.Services.CSV
{
    public class GenericCsvService<T> : IAccessCsvFile<T> where T : class
    {
        private readonly ClassMap<T> CustomClassMap;
        private readonly ILogger<GenericCsvService<T>> _logger;
        public GenericCsvService(ClassMap<T> classMap, ILogger<GenericCsvService<T>> logger) 
        {
            CustomClassMap = classMap;
            _logger = logger;
        }

        public IEnumerable<T> ReadSurveyResponses(IFormFile file, IEnumerable<string> expectedHeaders)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ReadSurveyResponsesWithFilterDate(IFormFile file, IEnumerable<string> expectedHeaders, string dateField, DateTime filterDate, 
            IAccessCsvFile<T>.FilterDateFunc filterDateFunc)
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
            csv.Context.RegisterClassMap(CustomClassMap);

            // Manually read records and filter rows before mapping
            var validRecords = new List<T>();
            while (csv.Read())
            {
                // Use the custom date filter logic (abstracted via delegate)
                string startDateField = csv.GetField(dateField); // Assuming StartDate field is in the CSV
                if (filterDateFunc(startDateField, filterDate))
                {
                    // Map the record if it passes the filter
                    var record = csv.GetRecord<T>();
                    validRecords.Add(record);
                }
            }

            return validRecords;
        }
    }
}
