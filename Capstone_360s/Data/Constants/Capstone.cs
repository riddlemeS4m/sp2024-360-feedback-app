using Capstone_360s.Models.FeedbackDb;
using System.Collections.ObjectModel;

namespace Capstone_360s.Data.Constants
{
    public class Capstone
    {
        public static readonly List<string> MetricKeys = 
        [
            "technology",
            "analytical",
            "communication",
            "participation",
            "performance"
        ];
        
        public static readonly List<string>  QuestionKeys = 
        [
            "strengths",
            "improvements",
            "comments"
        ];

        public static readonly string Technology = MetricKeys[0];
        public static readonly string Analytical = MetricKeys[1];
        public static readonly string Communication = MetricKeys[2];
        public static readonly string Participation = MetricKeys[3];
        public static readonly string Performance = MetricKeys[4];

        public static readonly string Strengths = QuestionKeys[0];
        public static readonly string Improvements = QuestionKeys[1];
        public static readonly string Comments = QuestionKeys[2];

        public static readonly List<string> CapstoneMetrics =
        [
            "Q2_1", "Q2_2", "Q2_3", "Q2_4", "Q2_5"
        ];

        public static readonly List<string> CapstoneQuestions = 
        [    
            "Q4", "Q5", "Q6", "Q10"
        ];

        public static readonly List<string> ExpectedHeaders = 
        [
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
        ];

        public static IEnumerable<Metric> GetDefaultCapstoneMetrics(Guid organizationId)
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

        public static IEnumerable<Question> GetDefaultCapstoneQuestions(Guid organizationId)
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

        public static bool CustomDateFilter(string dateField, DateTime filterDate)
        {
            // Try to parse the date and check if it is valid and meets the filter condition
            if (DateTime.TryParse(dateField, out DateTime parsedDate))
            {
                return parsedDate >= filterDate;
            }

            // If the date can't be parsed, we can exclude the row or handle it accordingly
            return false;
        }
    }
}
