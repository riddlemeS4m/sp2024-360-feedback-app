using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("projects")]
    public class Project
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        [Required]
        public Guid OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization Organization { get; set; }

        [Required]
        public int TimeframeId { get; set; }

        [ForeignKey(nameof(TimeframeId))]
        public Timeframe Timeframe { get; set; }

        public Guid? POCId { get; set; }

        [ForeignKey(nameof(POCId))]
        public User? POC { get; set; }

        public string? GDFolderId { get; set; }

        [Required]
        public int NoOfRounds { get; set; }

        public Guid? ManagerId { get; set; }

        [ForeignKey(nameof(ManagerId))]
        public User? Manager { get; set; }

        [Required]
        public int NoOfMembers { get; set; }

        [NotMapped]
        public List<Round> Rounds { get; set; }

        [NotMapped]
        public List<ProjectRound> ProjectRounds { get; set; }

        [NotMapped]
        public List<TeamMember> TeamMembers { get; set; }

        public override string ToString()
        {
            return $"{nameof(Project)}: Id = {Id}, Name = {Name}, Description = {Description}, OrganizationId = {OrganizationId}, TimeframeId = {TimeframeId}, POCId = {POCId}, GDFolderId = {GDFolderId}, NoOfRounds = {NoOfRounds}";
        }
    }
}
