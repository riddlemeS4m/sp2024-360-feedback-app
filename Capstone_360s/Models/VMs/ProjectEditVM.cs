using Capstone_360s.Models.FeedbackDb;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Capstone_360s.Models.VMs
{
    public class ProjectEditVM
    {
        public Project Project { get; set;}
        public List<SelectListItem> PotentialManagers { get; set; }
        public List<SelectListItem> PotentialPOCs { get; set; }
        public List<SelectListItem> PotentialTeamMembers { get; set; }
        public List<string> NewTeamMembers = new List<string>();
    }
}