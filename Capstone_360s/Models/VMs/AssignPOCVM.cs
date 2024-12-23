using Capstone_360s.Models.FeedbackDb;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Capstone_360s.Models.VMs
{
    public class AssignPOCVM
    {
        public Project Project { get; set; }
        public List<SelectListItem> POCs { get; set; }
    }
}