using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Models.VMs
{
    public class ProjectRoundsIndexVM
    {
        public IEnumerable<ProjectRound> ProjectRounds { get; set; } = new List<ProjectRound>();
        public string OrganizationId { get; set; }
        public int TimeframeId { get; set; }
    }
}
