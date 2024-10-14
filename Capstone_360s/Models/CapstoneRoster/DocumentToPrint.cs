using Capstone_360s.Models.Survey;

namespace Capstone_360s.Models.CapstoneRoster
{
    public class DocumentToPrint : GenericDocumentContent
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
        public string TimeframeName { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int RoundNumber { get; set; }
        public string RoundName { get; set; }
        public string[] Rounds { get; set; }
        public string[] Technical { get; set; }
        public string[] Analytical { get; set; }
        public string[] Communication { get; set; }
        public string[] Participation { get; set; }
        public string[] Performance { get; set; }
        public string[] Strengths { get; set; }
        public string[] AreasForImprovement { get; set; }
        public string[] Comments { get; set; }
        public string? ParentGDFolderId { get; set; }

        public DocumentToPrint() 
        { 
            this.FullName = this.FirstName + " " + this.LastName;
            this.RoundName = "Round " + this.RoundNumber;
        }
        
    }
}
