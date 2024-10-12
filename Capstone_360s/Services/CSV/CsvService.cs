using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.Roster;
using Capstone_360s.Utilities;
using CsvHelper;
using System.Globalization;

namespace Capstone_360s.Services.CSV
{
    public class CsvService : IAccessCsvFile
    {
        public readonly IReadOnlyList<string> CapstoneMetrics = new List<string>()
        {
            "Q2_1", "Q2_2", "Q2_3", "Q2_4", "Q2_5"
        };
        public readonly IReadOnlyList<string> CapstoneQuestions = new List<string>()
        {
            "Q4", "Q5", "Q6", "Q10"
        };

        private readonly ILogger<CsvService> _logger;
        public CsvService(ILogger<CsvService> logger) 
        {
            _logger = logger;
        }

        public IEnumerable<Metric> GetDefaultCapstoneMetrics(Guid organizationId)
        {
            return new List<Metric>()
            {
                new()
                {
                    OriginalMetricId = CapstoneMetrics[0],
                    Name = "Technology Score",
                    Description = "",
                    MinValue = 1,
                    MaxValue = 5,
                    Weight = 1,
                    OrganizationId = organizationId
                },
                new()
                {
                    OriginalMetricId = CapstoneMetrics[1],
                    Name = "Analytical Score",
                    Description = "",
                    MinValue = 1,
                    MaxValue = 5,
                    Weight = 1,
                    OrganizationId = organizationId
                },
                new()
                {
                    OriginalMetricId = CapstoneMetrics[2],
                    Name = "Communication Score",
                    Description = "",
                    MinValue = 1,
                    MaxValue = 5,
                    Weight = 1,
                    OrganizationId = organizationId
                },
                new()
                {
                    OriginalMetricId = CapstoneMetrics[3],
                    Name = "Participation Score",
                    Description = "",
                    MinValue = 1,
                    MaxValue = 5,
                    Weight = 1,
                    OrganizationId = organizationId
                },
                new()
                {
                    OriginalMetricId = CapstoneMetrics[4],
                    Name = "Performance Score",
                    Description = "",
                    MinValue = 1,
                    MaxValue = 5,
                    Weight = 1,
                    OrganizationId = organizationId
                }
            };
        }

        public IEnumerable<Question> GetDefaultCapstoneQuestions(Guid organizationId)
        {
            return new List<Question>()
            {
                new()
                {
                    OriginalQuestionId = CapstoneQuestions[0],
                    Q = "What strengths do you bring to your team?",
                    Example = "",
                    OrganizationId = organizationId
                },
                new()
                {
                    OriginalQuestionId = CapstoneQuestions[1],
                    Q = "What areas for growth exist?",
                    Example = "",
                    OrganizationId = organizationId
                },
                new()
                {
                    OriginalQuestionId = CapstoneQuestions[2],
                    Q = "Additional comments",
                    Example = "",
                    OrganizationId = organizationId
                },
                new()
                {
                    OriginalQuestionId = CapstoneQuestions[3],
                    Q = "What are your team members greatest strengths?",
                    Example = "",
                    OrganizationId = organizationId
                }
            };
        }

        public IEnumerable<Qualtrics> ReadCapstoneSurveyResponses(IFormFile file, DateTime filterDate)
        {
            IReadOnlyList<string> expectedHeaders = new List<string>
            {
                "StartDate", "EndDate", "Status", "IPAddress", "Progress",
                "Duration (in seconds)", "Finished", "RecordedDate", "ResponseId",
                "RecipientLastName", "RecipientFirstName", "RecipientEmail",
                "ExternalReference", "LocationLatitude", "LocationLongitude",
                "DistributionChannel", "UserLanguage",
                "Q2_1", "Q2_2", "Q2_3", "Q2_4", "Q2_5", "Q4", "Q5", "Q6",
                "Q8_1", "Q8_2", "Q8_3", "Q8_4", "Q8_5", "Q10", "Q11", "Q12", "Q13",
                "Q14_1", "Q14_2", "Q14_3", "Q14_4", "Q14_5", "Q16", "Q17", "Q18", "Q19",
                "Q20_1", "Q20_2", "Q20_3", "Q20_4", "Q20_5", "Q22", "Q23", "Q24", "Q25",
                "Q26_1", "Q26_2", "Q26_3", "Q26_4", "Q26_5", "Q28", "Q29", "Q30", "Q31",
                "Q32_1", "Q32_2", "Q32_3", "Q32_4", "Q32_5", "Q34", "Q35", "Q36", "Q38",
                "Q39_1", "Q39_2", "Q39_3", "Q39_4", "Q39_5", "Q41", "Q42", "Q43",
                "SC0", "FACULTYSPONSOR", "MEMBER1", "MEMBER2", "MEMBER3", "MEMBER4",
                "MEMBER5", "NUMTEAMMEMBER", "TEAMNAME", "TEAMNUM"
            };

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
            if (missingHeaders.Any())
            {
                throw new Exception($"The following expected headers are missing: {string.Join(", ", missingHeaders)}");
            }

            // Register the class map and read the records
            csv.Context.RegisterClassMap<SurveyResponseMap>();

            // Manually read records and filter rows before mapping
            var validRecords = new List<Qualtrics>();
            while (csv.Read())
            {
                // Try to parse the StartDate field before mapping the entire row
                if (DateTime.TryParse(csv.GetField(nameof(Qualtrics.StartDate)), out var parsedStartDate) && parsedStartDate >= filterDate)
                {
                    // If the date is valid and after the filter, map the row to a SurveyResponse
                    var record = csv.GetRecord<Qualtrics>();
                    validRecords.Add(record);
                }
            }

            return validRecords;
        }
    }
}
