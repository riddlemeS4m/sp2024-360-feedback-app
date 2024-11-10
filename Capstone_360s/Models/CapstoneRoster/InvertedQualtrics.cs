using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.Survey;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Capstone_360s.Models.CapstoneRoster
{
    public class InvertedQualtrics : GenericInversion
    {
        // Metadata about the response
        public Organization Organization { get; set; }
        public Timeframe Timeframe { get; set; }
        public Project Project { get; set; }
        public Round Round { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Recipient information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        // Reviewer information
        public string ReviewerEmail { get; set; }

        public List<Feedback> Feedback { get; set; }
        public Dictionary<string, int> Ratings { get; set; }
        public Dictionary<string, string> Questions { get; set; }

        public InvertedQualtrics() 
        {
            this.FullName = this.FirstName + " " + this.LastName;
        }

        public override string ToString()
        {
            return $"{nameof(InvertedQualtrics)}: Recipient = {Email}, Reviewer = {ReviewerEmail}";
        }
    }
}
