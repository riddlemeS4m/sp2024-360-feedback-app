namespace Capstone_360s.Models.VMs
{
    public class TimeframeCreateVM
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; }
        public int NoOfProjects { get; set; }
        public int NoOfRounds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> ProjectNames { get; set; } = new List<string>();
    }
}
