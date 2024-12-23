using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_360s.Models.FeedbackDb
{
    [Table("timeframes")]
    public class Timeframe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set;  }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [Required]
        public Guid OrganizationId { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public Organization Organization { get; set; }

        [DisplayName("Google Drive Folder")]
        public string? GDFolderId { get; set; }

        [Required]
        public int NoOfProjects { get; set; }

        [Required]
        public int NoOfRounds { get; set; }

        [DefaultValue(false)]
        public bool IsArchived { get; set; }

        [NotMapped]
        public List<Project> Projects { get; set; }

        public override string ToString()
        {
            return $"{nameof(Timeframe)}: Id = {Id}, Name = {Name}, StartDate = {StartDate}, EndDate = {EndDate}, OrganizationId = {OrganizationId}, GDFolderId = {GDFolderId}, NoOfProjects = {NoOfProjects}, IsArchived = {IsArchived}";
        }
    }
}
